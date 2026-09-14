using Rebus.Config;
using Shop.Orders.Contracts;
using Shop.Orders.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRebus(
    rebus => rebus.Transport(t =>
        t.UseRabbitMq(builder.Configuration["Rabbit:Url"], "orders-worker")),
    onCreated: async bus => await bus.Subscribe<OrderPlaced>());

builder.Services.AddRebusHandler<OrderPlacedHandler>();

var host = builder.Build();
host.Run();
