// Step 2: the shortcut an agent can take. Compiles fine. Breaks the architecture.
using Microsoft.EntityFrameworkCore;
using Shop.Domain;

namespace Shop.Application.Orders;

public class CancelOrderHandler(DbContext db)
{
    public async Task Handle(Guid id) =>
        (await db.Set<Order>().FindAsync(id))!.Cancel();
}
