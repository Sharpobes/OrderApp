using OrderApp.Domain.Constants;

namespace OrderApp.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ShipmentDate { get; set; }
    public int OrderNumber { get; set; }
    public string Status { get; set; } = OrderStatus.New;

    public Customer Customer { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}