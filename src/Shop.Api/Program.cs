using Microsoft.EntityFrameworkCore;
using Shop.Application.Abstractions;
using Shop.Application.Orders;
using Shop.Domain;
using Shop.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ShopDbContext>(o => o.UseInMemoryDatabase("shop"));
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<GetOrderHandler>();

var app = builder.Build();

// Seed one order so the demo has something to read.
using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
    await repo.SaveAsync(Order.Place(new Customer { Name = "Ada" }));
}

app.MapGet("/orders/{id:guid}", async (Guid id, GetOrderHandler handler) =>
    await handler.Handle(id) is { } order ? Results.Ok(order) : Results.NotFound());

app.Run();
