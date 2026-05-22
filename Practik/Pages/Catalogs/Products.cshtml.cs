using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Praktik.Domain.Entities;
using Praktik.Infrastructure.Data;

namespace Praktik.Pages.Catalogs;

public class ProductsModel : PageModel
{
    private readonly AppDbContext _context;
    public ProductsModel(AppDbContext context) => _context = context;

    public IList<Product> Products { get; set; }

    public async Task OnGetAsync()
    {
        Products = await _context.Products.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var p = await _context.Products.FindAsync(id);
        if (p != null) { _context.Products.Remove(p); await _context.SaveChangesAsync(); }
        return RedirectToPage();
    }
}