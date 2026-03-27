using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderApp.BusinessLogic.Ports;
using OrderApp.Domain.Constants;
using OrderApp.Web.DTOs.Orders;
using System.Security.Claims;

namespace OrderApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // Менеджер видит все заказы
    [HttpGet]
    [Authorize(Roles = Roles.Manager)]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var orders = await _orderService.GetAllOrdersAsync();

        if (!string.IsNullOrEmpty(status))
            orders = orders.Where(o => o.Status == status);

        return Ok(orders);
    }

    // Заказчик видит только свои заказы
    [HttpGet("my")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> GetMy([FromQuery] string? status)
    {
        var customerIdStr = User.FindFirstValue("CustomerId");
        if (customerIdStr == null)
            return BadRequest(new { message = "CustomerId не найден в токене" });

        var customerId = Guid.Parse(customerIdStr);
        var orders = await _orderService.GetOrdersByCustomerAsync(customerId);

        if (!string.IsNullOrEmpty(status))
            orders = orders.Where(o => o.Status == status);

        return Ok(orders);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> Create(CreateOrderDto dto)
    {
        var customerIdStr = User.FindFirstValue("CustomerId");
    
        if (customerIdStr == null)
            return BadRequest(new { message = "CustomerId не найден в токене" });

        var customerId = Guid.Parse(customerIdStr);
        var items = dto.Items.Select(i => (i.ItemId, i.Count)).ToList();
        var order = await _orderService.CreateOrderAsync(customerId, items);
        return Ok(order);
    }

    [HttpPut("{id}/confirm")]
    [Authorize(Roles = Roles.Manager)]
    public async Task<IActionResult> Confirm(Guid id, ConfirmOrderDto dto)
    {
        await _orderService.ConfirmOrderAsync(id, dto.ShipmentDate);
        return Ok();
    }

    // Менеджер закрывает заказ
    [HttpPut("{id}/complete")]
    [Authorize(Roles = Roles.Manager)]
    public async Task<IActionResult> Complete(Guid id)
    {
        await _orderService.CompleteOrderAsync(id);
        return Ok();
    }

    // Заказчик удаляет заказ (только со статусом Новый)
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _orderService.DeleteOrderAsync(id);
        return Ok();
    }
}