// The fix the failing test message points to: depend on the abstraction, not EF Core.
using Shop.Application.Abstractions;

namespace Shop.Application.Orders;

public class CancelOrderHandler(IOrderRepository orders)
{
    public async Task Handle(Guid id)
    {
        var order = await orders.GetAsync(id)
            ?? throw new KeyNotFoundException($"Order {id} not found.");

        order.Cancel();
        await orders.SaveAsync(order);
    }
}
