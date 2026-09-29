using Microsoft.EntityFrameworkCore;
using Rebus.Bus;
using Rebus.Config;
using Shop.Orders.Api;
using Shop.Orders.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrdersDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Db")));

// Publishes only, so no input queue of its own.
builder.Services.AddRebus(rebus => rebus
    .Transport(t => t.UseRabbitMqAsOneWayClient(builder.Configuration["Rabbit:Url"])));

// Orders prices every order from Catalog, so it has to be able to reach it.
builder.Services.AddHttpClient("catalog", http =>
    http.BaseAddress = new Uri(builder.Configuration["Catalog:Url"]!));

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseCors();

await DbBootstrap.EnsureCreatedAsync(app.Services, app.Logger);

app.MapGet("/", () => "Shop.Orders API — hello from .NET 10");

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "orders-api" }));

app.MapGet("/orders", async (OrdersDbContext db) =>
    await db.Orders.OrderByDescending(o => o.Id).ToListAsync());

app.MapPost("/orders", async (PlaceOrder request, OrdersDbContext db, IBus bus, IHttpClientFactory clients) =>
{
    var catalog = clients.CreateClient("catalog");
    var product = await catalog.GetFromJsonAsync<Product>($"/products/{request.ProductId}")
        ?? throw new InvalidOperationException($"Catalog has no product #{request.ProductId}");

    var price = Math.Round(product.Price * 0.9m, 2);
    var order = new Order(0, product.Id, product.Name, price, request.Quantity);
    db.Orders.Add(order);
    await db.SaveChangesAsync();

    await bus.Publish(new OrderPlaced(order.Id, order.ProductName, order.Quantity));
    app.Logger.LogInformation("📤 published OrderPlaced — #{Id} {ProductName} x{Quantity}",
        order.Id, order.ProductName, order.Quantity);

    return Results.Created($"/orders/{order.Id}", order);
});

app.Run();
