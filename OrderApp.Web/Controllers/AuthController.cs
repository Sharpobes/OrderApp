using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OrderApp.Web.DTOs.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using OrderApp.DataAccess.UnitOfWork;
using OrderApp.Domain.Entities;

namespace OrderApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _config;
    private readonly IUnitOfWork _uow;
    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration config,
        IUnitOfWork uow)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
        _uow = uow;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var user = new IdentityUser { UserName = dto.Login };
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, "Customer");

        // Создаём запись Customer автоматически
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = dto.Login,
            Code = $"{Random.Shared.Next(1000, 9999)}-{DateTime.UtcNow.Year}",
            Address = "",
            Discount = null
        };

        await _uow.Customers.AddAsync(customer);
        await _uow.SaveChangesAsync();

        // Сохраняем CustomerId в токен
        await _userManager.AddClaimAsync(user,
            new Claim("CustomerId", customer.Id.ToString()));

        return Ok(new { message = "Регистрация успешна" });
    }

    // Любой может войти
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.Login);
        if (user == null)
            return Unauthorized(new { message = "Неверный логин или пароль" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            return Unauthorized(new { message = "Неверный логин или пароль" });

        var token = await GenerateTokenAsync(user);
        return Ok(new { token });
    }

    private async Task<string> GenerateTokenAsync(IdentityUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var userClaims = await _userManager.GetClaimsAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!)
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(userClaims);

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}