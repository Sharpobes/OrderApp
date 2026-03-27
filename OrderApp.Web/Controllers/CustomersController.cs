using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderApp.DataAccess.UnitOfWork;
using OrderApp.Domain.Constants;
using OrderApp.Domain.Entities;
using OrderApp.Web.DTOs.Customers;

namespace OrderApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Manager)]
public class CustomersController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public CustomersController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _uow.Customers.GetAllAsync();
        return Ok(customers.Select(c => new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            Code = c.Code,
            Address = c.Address,
            Discount = c.Discount
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var customer = await _uow.Customers.GetByIdAsync(id);
        if (customer == null) return NotFound();

        return Ok(new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Code = customer.Code,
            Address = customer.Address,
            Discount = customer.Discount
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerDto dto)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Code = dto.Code,
            Address = dto.Address,
            Discount = dto.Discount
        };

        await _uow.Customers.AddAsync(customer);
        await _uow.SaveChangesAsync();

        return Ok(new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Code = customer.Code,
            Address = customer.Address,
            Discount = customer.Discount
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, CreateCustomerDto dto)
    {
        var customer = await _uow.Customers.GetByIdAsync(id);
        if (customer == null) return NotFound();

        customer.Name = dto.Name;
        customer.Code = dto.Code;
        customer.Address = dto.Address;
        customer.Discount = dto.Discount;

        _uow.Customers.Update(customer);
        await _uow.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var customer = await _uow.Customers.GetByIdAsync(id);
        if (customer == null) return NotFound();

        _uow.Customers.Delete(customer);
        await _uow.SaveChangesAsync();
        return Ok();
    }
}