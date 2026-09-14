using Rebus.Handlers;
using Shop.Orders.Contracts;

namespace Shop.Orders.Worker;

// One method, one message type — Rebus does the queue plumbing.
public class OrderPlacedHandler(ILogger<OrderPlacedHandler> logger)
    : IHandleMessages<OrderPlaced>
{
    public Task Handle(OrderPlaced message)
    {
        logger.LogInformation("🧾 order.placed — #{Id} {ProductName} x{Quantity}",
            message.Id, message.ProductName, message.Quantity);
        return Task.CompletedTask;
    }
}
