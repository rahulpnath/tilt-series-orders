namespace Shop.Orders.Contracts;

// Published by the API when an order is saved, handled by the worker.
public record OrderPlaced(int Id, string ProductName, int Quantity);
