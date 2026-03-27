using Microsoft.EntityFrameworkCore;
using OrderApp.BusinessLogic.Ports;
using OrderApp.DataAccess;
using OrderApp.DataAccess.UnitOfWork;
using OrderApp.Domain.Entities;

namespace OrderApp.BusinessLogic.Adapters;

public class ItemService : IItemService
{
    private readonly IUnitOfWork _uow;
    private readonly AppDbContext _context;

    public ItemService(IUnitOfWork uow, AppDbContext context)
    {
        _uow = uow;
        _context = context;
    }

    public async Task<IEnumerable<Item>> GetAllItemsAsync() =>
        await _context.Items.ToListAsync();

    public async Task<Item?> GetItemByIdAsync(Guid id) =>
        await _uow.Items.GetByIdAsync(id);

    public async Task AddItemAsync(Item item)
    {
        item.Id = Guid.NewGuid();
        await _uow.Items.AddAsync(item);
        await _uow.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(Item item)
    {
        _uow.Items.Update(item);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(Guid id)
    {
        var item = await _uow.Items.GetByIdAsync(id)
                   ?? throw new Exception("Товар не найден");

        _uow.Items.Delete(item);
        await _uow.SaveChangesAsync();
    }
}