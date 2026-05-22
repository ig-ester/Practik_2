using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Praktik.Domain.Entities;
using Praktik.Infrastructure.Data;

namespace Praktik.Pages.Catalogs;

public class WarehousesModel : PageModel
{
    private readonly AppDbContext _context;

    public WarehousesModel(AppDbContext context)
    {
        _context = context;
    }

    public IList<Warehouse> Warehouses { get; set; } = new List<Warehouse>();

    public async Task OnGetAsync()
    {
        Warehouses = await _context.Warehouses
            .Include(w => w.Organization)
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var warehouse = await _context.Warehouses.FindAsync(id);
        if (warehouse != null)
        {
            _context.Warehouses.Remove(warehouse);
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}