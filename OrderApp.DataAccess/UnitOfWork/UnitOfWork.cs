using OrderApp.DataAccess.Repositories;
using OrderApp.Domain.Entities;

namespace OrderApp.DataAccess.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IRepository<Customer> Customers { get; }
    public IRepository<Order> Orders { get; }
    public IRepository<OrderItem> OrderItems { get; }
    public IRepository<Item> Items { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Customers = new Repository<Customer>(context);
        Orders = new Repository<Order>(context);
        OrderItems = new Repository<OrderItem>(context);
        Items = new Repository<Item>(context);
    }

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}