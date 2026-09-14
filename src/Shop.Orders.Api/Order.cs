namespace Shop.Orders.Api;

public record Order(int Id, int ProductId, string ProductName, decimal UnitPrice, int Quantity);

public record PlaceOrder(int ProductId, int Quantity);

// What Catalog's GET /products/{id} returns.
public record Product(int Id, string Name, decimal Price);
