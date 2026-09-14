namespace Shop.Orders.Contracts;

// Its own project because both sides need the same type: Rebus puts the type
// name on the wire.
public record OrderPlaced(int Id, string ProductName, int Quantity);
