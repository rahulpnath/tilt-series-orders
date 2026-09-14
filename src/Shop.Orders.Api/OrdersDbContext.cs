using Microsoft.EntityFrameworkCore;

namespace Shop.Orders.Api;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    // Without an explicit precision SQL Server truncates the decimal silently.
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Order>().Property(o => o.UnitPrice).HasPrecision(18, 2);
}
