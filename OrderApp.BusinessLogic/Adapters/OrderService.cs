using Microsoft.EntityFrameworkCore;
using OrderApp.BusinessLogic.Ports;
using OrderApp.DataAccess;
using OrderApp.DataAccess.UnitOfWork;
using OrderApp.Domain.Constants;
using OrderApp.Domain.Entities;

namespace OrderApp.BusinessLogic.Adapters;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly AppDbContext _context;

    public OrderService(IUnitOfWork uow, AppDbContext context)
    {
        _uow = uow;
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync() =>
        await _context.Orders.Include(o => o.Customer)
                              .Include(o => o.OrderItems)
                              .ThenInclude(oi => oi.Item)
                              .ToListAsync();

    public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId) =>
        await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Item)
            .ToListAsync();

    public async Task<Order> CreateOrderAsync(Guid customerId, List<(Guid itemId, int count)> items)
    {
        var lastNumber = await _context.Orders.MaxAsync(o => (int?)o.OrderNumber) ?? 0;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            OrderNumber = lastNumber + 1,
            Status = OrderStatus.New
        };

        await _uow.Orders.AddAsync(order);

        foreach (var (itemId, count) in items)
        {
            var item = await _uow.Items.GetByIdAsync(itemId)
                ?? throw new Exception("Товар не найден");

            order.OrderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ItemId = itemId,
                ItemsCount = count,
                ItemPrice = item.Price
            });
        }

        await _uow.SaveChangesAsync();
        return order;
    }

    public async Task ConfirmOrderAsync(Guid orderId, DateTime shipmentDate)
    {
        var order = await _uow.Orders.GetByIdAsync(orderId)
                    ?? throw new Exception("Заказ не найден");

        order.Status = OrderStatus.InProgress;
        order.ShipmentDate = DateTime.SpecifyKind(shipmentDate, DateTimeKind.Utc);
        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync();
    }

    public async Task CompleteOrderAsync(Guid orderId)
    {
        var order = await _uow.Orders.GetByIdAsync(orderId)
            ?? throw new Exception("Заказ не найден");

        order.Status = OrderStatus.Completed;
        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteOrderAsync(Guid orderId)
    {
        var order = await _uow.Orders.GetByIdAsync(orderId)
            ?? throw new Exception("Заказ не найден");

        if (order.Status != OrderStatus.New)
            throw new InvalidOperationException("Можно удалить только заказ со статусом 'Новый'");

        _uow.Orders.Delete(order);
        await _uow.SaveChangesAsync();
    }
}