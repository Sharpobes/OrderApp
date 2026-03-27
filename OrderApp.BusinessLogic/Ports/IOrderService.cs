using OrderApp.Domain.Entities;

namespace OrderApp.BusinessLogic.Ports;

public interface IOrderService
{
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId);
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task<Order> CreateOrderAsync(Guid customerId, List<(Guid itemId, int count)> items);
    Task ConfirmOrderAsync(Guid orderId, DateTime shipmentDate);
    Task CompleteOrderAsync(Guid orderId);
    Task DeleteOrderAsync(Guid orderId);
}