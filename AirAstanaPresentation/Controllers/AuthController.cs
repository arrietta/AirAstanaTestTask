using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Presentation.Services;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AirAstanaDbContext _db;
    private readonly JwtTokenService _jwt;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AirAstanaDbContext db, JwtTokenService jwt, ILogger<AuthController> logger)
    {
        _db = db;
        _jwt = jwt;
        _logger = logger;
    }

    /// <summary>Authorization also registration if there is no user in data with username like this</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
        {
            var userRole = await _db.Roles.FirstAsync(r => r.Code == "User");
            user = new User
            {
                Username = request.Username,
                Password = ToHash(request.Password),
                RoleId = userRole.Id,
                Role = userRole
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "{User} at {Time}: new user",
                user.Username,
                DateTimeOffset.UtcNow);
        }
        else
        {
            var hash = ToHash(request.Password);
            if (user.Password != hash) return Unauthorized();
        }

        var token = _jwt.GenerateToken(user);
        return Ok(new { token });
    }

    /// <summary>Get moderator role for this user</summary>
    [HttpPut("role/moderator")]
    [Authorize]
    public async Task<IActionResult> GetModeratorRole()
    {
        return await ChangeCurrentUserRole("Moderator");
    }

    /// <summary>Get user role for this user</summary>
    [HttpPut("role/user")]
    [Authorize]
    public async Task<IActionResult> GetUserRole()
    {
        return await ChangeCurrentUserRole("User");
    }

    private async Task<IActionResult> ChangeCurrentUserRole(string roleCode)
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(username))
        {
            return Unauthorized();
        }

        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Username == username);
        if (user is null)
        {
            return Unauthorized();
        }

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Code == roleCode);

        user.RoleId = role.Id;
        user.Role = role;
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "{User} at {Time}: role to {Role}",
            username,
            DateTimeOffset.UtcNow,
            roleCode);

        var token = _jwt.GenerateToken(user);
        return Ok(new { token });
    }

    private static string ToHash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }
}

public record LoginRequest(string Username, string Password);