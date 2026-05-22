using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Praktik.Domain.Entities;
using Praktik.Infrastructure.Data;

namespace Praktik.Pages.Catalogs;

public class UserEditModel : PageModel
{
    private readonly AppDbContext _context;

    public UserEditModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public new User User { get; set; } = new User();

    public IList<Role> Roles { get; set; } = new List<Role>();
    public IList<Organization> Organizations { get; set; } = new List<Organization>();

    [BindProperty]
    public string? Password { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        Roles = await _context.Roles.ToListAsync();
        Organizations = await _context.Organizations.ToListAsync();

        if (id.HasValue)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();
            User = user;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {

        try
        {
            if (User.Id == 0)
            {
                if (string.IsNullOrEmpty(Password))
                {
                    ModelState.AddModelError("Password", "Пароль обязателен для нового пользователя");
                    Roles = await _context.Roles.ToListAsync();
                    Organizations = await _context.Organizations.ToListAsync();
                    return Page();
                }

                User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password);
                User.CreatedAt = DateTime.UtcNow;
                User.IsActive = true;
                User.LastLogin = null;

                _context.Users.Add(User);
            }
            else
            {
                var existingUser = await _context.Users.FindAsync(User.Id);
                if (existingUser == null)
                    return NotFound();

                existingUser.RoleId = User.RoleId;
                existingUser.OrganizationId = User.OrganizationId;
                existingUser.IsActive = User.IsActive;

                if (!string.IsNullOrEmpty(Password))
                {
                    existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("Users");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка сохранения: {ex.Message}");
            Roles = await _context.Roles.ToListAsync();
            Organizations = await _context.Organizations.ToListAsync();
            return Page();
        }
    }
}