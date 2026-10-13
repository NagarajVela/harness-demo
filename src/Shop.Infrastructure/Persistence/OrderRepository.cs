using Shop.Application.Abstractions;
using Shop.Domain;

namespace Shop.Infrastructure.Persistence;

public class OrderRepository(ShopDbContext db) : IOrderRepository
{
    public async Task<Order?> GetAsync(Guid id) => await db.Orders.FindAsync(id);

    public async Task SaveAsync(Order order)
    {
        if (db.Entry(order).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
            db.Orders.Add(order);

        await db.SaveChangesAsync();
    }
}
