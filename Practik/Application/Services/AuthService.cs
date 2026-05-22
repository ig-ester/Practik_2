using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Praktik.Infrastructure.Data;
using Praktik.Domain.Entities;

namespace Praktik.Application.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public AuthService(AppDbContext db, IHttpContextAccessor http) { _db = db; _http = http; }

    public async Task<(bool Success, string? Error, User? User)> ValidateUserAsync(string login, string password)
    {
        var user = await _db.Users.Include(u => u.Role).Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Login == login && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, "Неверный логин или пароль", null);

        user.LastLogin = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Login),
            new(ClaimTypes.Role, user.Role.Name),
            new("OrganizationId", user.OrganizationId.ToString())
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await _http.HttpContext!.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return (true, null, user);
    }

    public async Task SignOutAsync() => await _http.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
}