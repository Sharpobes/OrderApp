using OrderApp.DataAccess.Repositories;
using OrderApp.Domain.Entities;

namespace OrderApp.DataAccess.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IRepository<Customer> Customers { get; }
    IRepository<Order> Orders { get; }
    IRepository<OrderItem> OrderItems { get; }
    IRepository<Item> Items { get; }
    Task<int> SaveChangesAsync();
}