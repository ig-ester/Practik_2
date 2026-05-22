using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Praktik.Domain.Entities;
using Praktik.Infrastructure.Data;

namespace Praktik.Pages.Catalogs;

public class ProductEditModel : PageModel
{
    private readonly AppDbContext _context;

    public ProductEditModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Product Product { get; set; } = new Product();

    public IActionResult OnGet(int? id)
    {
        if (id.HasValue)
        {
            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound();
            Product = product;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        if (Product.Id == 0)
        {
            Product.CreatedAt = DateTime.UtcNow;
            Product.UpdatedAt = DateTime.UtcNow; 
            Product.CurrentStock = 0; 
            _context.Products.Add(Product);
        }
        else
        {
            Product.UpdatedAt = DateTime.UtcNow; 
            _context.Products.Update(Product);
        }

        await _context.SaveChangesAsync();
        return RedirectToPage("Products");
    }
}