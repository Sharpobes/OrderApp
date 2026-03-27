namespace OrderApp.Web.DTOs.Customers;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Address { get; set; } = null!;
    public decimal? Discount { get; set; }
}