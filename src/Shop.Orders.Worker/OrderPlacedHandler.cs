using Rebus.Handlers;
using Shop.Orders.Contracts;

namespace Shop.Orders.Worker;

// Logs a receipt for every order.
public class OrderPlacedHandler(ILogger<OrderPlacedHandler> logger)
    : IHandleMessages<OrderPlaced>
{
    public Task Handle(OrderPlaced message)
    {
        logger.LogInformation("🧾 order.placed — receipt for #{Id}: {Quantity} x {ProductName}",
            message.Id, message.Quantity, message.ProductName);
        return Task.CompletedTask;
    }
}
