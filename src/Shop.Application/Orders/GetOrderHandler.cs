using Shop.Application.Abstractions;

namespace Shop.Application.Orders;

public record OrderDto(Guid Id, string Status);

public class GetOrderHandler(IOrderRepository orders)
{
    public async Task<OrderDto?> Handle(Guid id)
    {
        var order = await orders.GetAsync(id);
        return order is null ? null : new OrderDto(order.Id, order.Status.ToString());
    }
}
