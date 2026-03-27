using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderApp.BusinessLogic.Ports;
using OrderApp.Domain.Constants;
using OrderApp.Domain.Entities;
using OrderApp.Web.DTOs.Items;

namespace OrderApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;

    public ItemsController(IItemService itemService)
    {
        _itemService = itemService;
    }

    // Все могут смотреть каталог
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _itemService.GetAllItemsAsync();
        var result = items.Select(i => new ItemDto
        {
            Id = i.Id,
            Code = i.Code,
            Name = i.Name,
            Price = i.Price,
            Category = i.Category
        });
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _itemService.GetItemByIdAsync(id);
        if (item == null) return NotFound();

        return Ok(new ItemDto
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            Price = item.Price,
            Category = item.Category
        });
    }

    // Только менеджер может добавлять/редактировать/удалять
    [HttpPost]
    [Authorize(Roles = Roles.Manager)]
    public async Task<IActionResult> Create(CreateItemDto dto)
    {
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Code = dto.Code,
            Name = dto.Name,
            Price = dto.Price,
            Category = dto.Category
        };

        await _itemService.AddItemAsync(item);
        return Ok(new ItemDto
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            Price = item.Price,
            Category = item.Category
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Manager)]
    public async Task<IActionResult> Update(Guid id, CreateItemDto dto)
    {
        var item = await _itemService.GetItemByIdAsync(id);
        if (item == null) return NotFound();

        item.Code = dto.Code;
        item.Name = dto.Name;
        item.Price = dto.Price;
        item.Category = dto.Category;

        await _itemService.UpdateItemAsync(item);
        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Manager)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _itemService.DeleteItemAsync(id);
        return Ok();
    }
}