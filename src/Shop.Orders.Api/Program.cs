using Microsoft.EntityFrameworkCore;
using Rebus.Bus;
using Rebus.Config;
using Shop.Orders.Api;
using Shop.Orders.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrdersDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Db")));

builder.Services.AddRebus(rebus => rebus
    .Transport(t => t.UseRabbitMqAsOneWayClient(builder.Configuration["Rabbit:Url"])));

builder.Services.AddHttpClient();

// Local development only: the Storefront is a different origin, so without
// this the browser blocks every call.
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors();

await DbBootstrap.EnsureCreatedAsync(app.Services, app.Logger);

app.MapGet("/", () => "Shop.Orders API — hello from .NET 10");

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "orders-api" }));

app.MapGet("/orders", async (OrdersDbContext db) =>
    await db.Orders.OrderByDescending(o => o.Id).ToListAsync());

// Reaches Catalog by name over the Docker network, and returns the failure
// verbatim when it can't.
app.MapGet("/catalog-check", async (IHttpClientFactory factory, IConfiguration config) =>
{
    var url = config["Catalog:BaseUrl"] ?? "http://catalog-api:5101";
    var http = factory.CreateClient();
    http.Timeout = TimeSpan.FromSeconds(5);
    try
    {
        var body = await http.GetStringAsync($"{url}/products");
        return Results.Ok(new { reached = url, ok = true, products = body });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { reached = url, ok = false, error = ex.Message });
    }
});

app.MapPost("/orders", async (Order order, OrdersDbContext db, IBus bus,
    IHttpClientFactory factory, IConfiguration config, ILoggerFactory loggers) =>
{
    var log = loggers.CreateLogger("Orders");

    // Best-effort: an order is still placeable while Catalog is unreachable.
    var catalogUrl = config["Catalog:BaseUrl"] ?? "http://catalog-api:5101";
    try
    {
        var http = factory.CreateClient();
        http.Timeout = TimeSpan.FromSeconds(3);
        var products = await http.GetStringAsync($"{catalogUrl}/products");
        log.LogInformation("Catalog reachable at {Url} ({Bytes} bytes)", catalogUrl, products.Length);
    }
    catch (Exception ex)
    {
        log.LogWarning("Catalog call to {Url} failed (continuing): {Message}", catalogUrl, ex.Message);
    }

    db.Orders.Add(order);
    await db.SaveChangesAsync();

    await bus.Publish(new OrderPlaced(order.Id, order.ProductName, order.Quantity));

    return Results.Created($"/orders/{order.Id}", order);
});

app.Run();
