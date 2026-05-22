using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Praktik.Domain.Entities;
using Praktik.Infrastructure.Data;

namespace Praktik.Pages.Catalogs;

public class WarehouseEditModel : PageModel
{
    private readonly AppDbContext _context;

    public WarehouseEditModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Warehouse Warehouse { get; set; } = new Warehouse();

    public IList<Organization> Organizations { get; set; } = new List<Organization>();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        Organizations = await _context.Organizations.OrderBy(o => o.Name).ToListAsync();

        if (id.HasValue && id > 0)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null) return NotFound();
            Warehouse = warehouse;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Organizations = await _context.Organizations.ToListAsync();

        if (string.IsNullOrWhiteSpace(Warehouse.Name))
            ModelState.AddModelError("Warehouse.Name", "Укажите название склада");

        if (Warehouse.OrganizationId <= 0)
            ModelState.AddModelError("Warehouse.OrganizationId", "Выберите организацию из списка");
        //if (!ModelState.IsValid)
        //   return Page();

        try
        {
            if (Warehouse.Id == 0)
            {
                _context.Warehouses.Add(Warehouse);
                TempData["Success"] = "Склад успешно создан";
            }
            else
            {
                var existing = await _context.Warehouses.FindAsync(Warehouse.Id);
                if (existing == null) return NotFound();

                existing.Name = Warehouse.Name;
                existing.Type = Warehouse.Type;
                existing.OrganizationId = Warehouse.OrganizationId;
                TempData["Success"] = "Склад успешно обновлён";
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("Warehouses");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка БД: {ex.InnerException?.Message ?? ex.Message}");
            return Page();
        }
    }
}