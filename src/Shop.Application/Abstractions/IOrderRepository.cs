using Shop.Domain;

namespace Shop.Application.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetAsync(Guid id);

    Task SaveAsync(Order order);
}
