namespace OrderApp.Web.DTOs.Customers;

public class CreateCustomerDto
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Address { get; set; } = null!;
    public decimal? Discount { get; set; }
}