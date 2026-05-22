using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Praktik.Domain.Entities;
using Praktik.Infrastructure.Data;

namespace Praktik.Pages.Catalogs;

public class OrganizationEditModel : PageModel
{
    private readonly AppDbContext _context;

    public OrganizationEditModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Organization Organization { get; set; } = new Organization();

    public IActionResult OnGet(int? id)
    {
        if (id.HasValue)
        {
            var org = _context.Organizations.Find(id);
            if (org == null)
                return NotFound();
            Organization = org;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            if (Organization.Id == 0)
            {
                Organization.CreatedAt = DateTime.UtcNow;
                _context.Organizations.Add(Organization);
            }
            else
            {
                var existing = await _context.Organizations.FindAsync(Organization.Id);
                if (existing == null)
                    return NotFound();

                existing.Name = Organization.Name;
                existing.Address = Organization.Address;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("Organizations");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка сохранения: {ex.Message}");
            return Page();
        }
    }
}