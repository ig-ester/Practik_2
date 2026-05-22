using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Praktik.Domain.Entities;
using Praktik.Infrastructure.Data;

namespace Praktik.Pages.Catalogs;

public class OrganizationsModel : PageModel
{
    private readonly AppDbContext _context;

    public OrganizationsModel(AppDbContext context)
    {
        _context = context;
    }

    public IList<Organization> Organizations { get; set; } = new List<Organization>();

    public async Task OnGetAsync()
    {
        Organizations = await _context.Organizations
            .OrderBy(o => o.Name)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var org = await _context.Organizations.FindAsync(id);
            if (org != null)
            {
                bool hasWarehouses = await _context.Warehouses.AnyAsync(w => w.OrganizationId == id);
                bool hasUsers = await _context.Users.AnyAsync(u => u.OrganizationId == id);

                if (hasWarehouses || hasUsers)
                {
                    TempData["Error"] = "Невозможно удалить организацию: имеются связанные склады или пользователи";
                    return RedirectToPage();
                }

                _context.Organizations.Remove(org);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Организация успешно удалена";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ошибка удаления: {ex.Message}";
        }
        return RedirectToPage();
    }
}