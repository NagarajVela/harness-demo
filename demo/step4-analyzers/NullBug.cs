// Step 4: a null-dereference an agent can easily write. Customer is Customer?.
using Shop.Domain;

namespace Shop.Application.Orders;

internal static class NullBug
{
    public static string CustomerName(Order order)
    {
        var name = order.Customer.Name;
        return name;
    }
}
