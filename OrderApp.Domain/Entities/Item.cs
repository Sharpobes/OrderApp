namespace OrderApp.Domain.Entities;

public class Item
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;  // XX-XXXX-YYXX
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public string Category { get; set; } = null!;  // max 30 символов
}