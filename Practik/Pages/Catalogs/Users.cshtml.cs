using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Praktik.Domain.Entities;
using Praktik.Infrastructure.Data;

namespace Praktik.Pages.Catalogs;

public class UsersModel : PageModel
{
    private readonly AppDbContext _context;

    public UsersModel(AppDbContext context)
    {
        _context = context;
    }

    public IList<User> Users { get; set; } = new List<User>();

    public async Task OnGetAsync()
    {
        Users = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Organization)
            .OrderBy(u => u.Login)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}