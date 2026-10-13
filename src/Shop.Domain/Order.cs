namespace Shop.Domain;

public class Customer
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "";
}

public enum OrderStatus
{
    Placed,
    Shipped,
    Cancelled
}

public class Order
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    // Nullable on purpose: guest checkout has no customer. Step 4 of the demo relies on this.
    public Customer? Customer { get; private set; }

    public OrderStatus Status { get; private set; } = OrderStatus.Placed;

    public static Order Place(Customer? customer) => new() { Customer = customer };

    public void Cancel()
    {
        if (Status == OrderStatus.Shipped)
            throw new InvalidOperationException("A shipped order cannot be cancelled.");

        Status = OrderStatus.Cancelled;
    }
}
