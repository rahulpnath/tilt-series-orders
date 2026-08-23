using Microsoft.EntityFrameworkCore;

namespace Shop.Orders.Api;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
}
