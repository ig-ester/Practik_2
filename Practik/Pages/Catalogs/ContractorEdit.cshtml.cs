using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Praktik.Domain.Entities;
using Praktik.Infrastructure.Data;

namespace Praktik.Pages.Catalogs;

public class ContractorEditModel : PageModel
{
    private readonly AppDbContext _context;

    public ContractorEditModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Contractor Contractor { get; set; } = new Contractor();

    public IActionResult OnGet(int? id)
    {
        if (id.HasValue)
        {
            var contractor = _context.Contractors.Find(id);
            if (contractor == null)
                return NotFound();
            Contractor = contractor;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        if (Contractor.Id == 0)
            _context.Contractors.Add(Contractor);
        else
            _context.Contractors.Update(Contractor);

        await _context.SaveChangesAsync();
        return RedirectToPage("Contractors");
    }
}