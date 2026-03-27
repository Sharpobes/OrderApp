namespace OrderApp.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;  // XXXX-ГГГГ
    public string Address { get; set; } = null!;
    public decimal? Discount { get; set; }      // null = нет скидки

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}