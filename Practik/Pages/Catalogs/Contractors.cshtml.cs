using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Praktik.Domain.Entities;
using Praktik.Infrastructure.Data;

namespace Praktik.Pages.Catalogs;

public class ContractorsModel : PageModel
{
    private readonly AppDbContext _context;

    public ContractorsModel(AppDbContext context)
    {
        _context = context;
    }

    public IList<Contractor> Contractors { get; set; } = new List<Contractor>();
    public string? SearchTerm { get; set; }
    public string? TypeFilter { get; set; }

    public async Task OnGetAsync(string? searchTerm, string? type)
    {
        SearchTerm = searchTerm;
        TypeFilter = type;

        var query = _context.Contractors.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(c => c.Name.Contains(searchTerm) ||
                                     (c.INN != null && c.INN.Contains(searchTerm)));
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(c => c.Type == type);
        }

        Contractors = await query.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var contractor = await _context.Contractors.FindAsync(id);
        if (contractor != null)
        {
            _context.Contractors.Remove(contractor);
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}