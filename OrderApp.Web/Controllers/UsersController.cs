using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrderApp.Domain.Constants;
using OrderApp.Web.DTOs.Users;

namespace OrderApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Manager)]
public class UsersController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;

    public UsersController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = new List<object>();
        foreach (var user in _userManager.Users.ToList())
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var customerId = claims.FirstOrDefault(c => c.Type == "CustomerId")?.Value;
        
            users.Add(new
            {
                user.Id,
                user.UserName,
                Roles = roles,
                CustomerId = customerId
            });
        }
        return Ok(users);
    }
    [HttpPost("{id}/roles")]
    public async Task<IActionResult> SetRole(string id, [FromBody] SetRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        // Убираем все текущие роли
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        // Назначаем новую
        var result = await _userManager.AddToRoleAsync(user, dto.Role);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = $"Роль {dto.Role} назначена" });
    }
    [HttpPost("{id}/customer")]
    public async Task<IActionResult> SetCustomer(string id, [FromBody] SetCustomerDto dto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        // Удаляем старый CustomerId если был
        var existingClaims = await _userManager.GetClaimsAsync(user);
        var oldClaim = existingClaims.FirstOrDefault(c => c.Type == "CustomerId");
        if (oldClaim != null)
            await _userManager.RemoveClaimAsync(user, oldClaim);

        // Добавляем новый
        await _userManager.AddClaimAsync(user,
            new Claim("CustomerId", dto.CustomerId.ToString()));

        return Ok(new { message = "Заказчик привязан" });
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        await _userManager.DeleteAsync(user);
        return Ok();
    }
}