using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Praktik.Domain.Entities;
using Praktik.Domain.Enums;
using Praktik.Infrastructure.Data;

namespace Praktik.Pages.Documents.Receiving;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public IList<DocumentViewModel> Documents { get; set; } = new List<DocumentViewModel>();
    public string? SearchTerm { get; set; }
    public string? StatusFilter { get; set; }

    public class DocumentViewModel
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty; 
        public string Warehouse { get; set; } = string.Empty;
        public string? Contractor { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemsCount { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public async Task OnGetAsync(string? searchTerm, string? status)
    {
        SearchTerm = searchTerm;
        StatusFilter = status;

        var query = _context.Documents
            .Include(d => d.Warehouse)
            .Include(d => d.Contractors)
            .Include(d => d.CreatedByUser)
            .Include(d => d.Items)
            .Where(d => d.Type == "Receiving") 
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(d => d.Number.Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(d => d.Status == status);
        }

        var docs = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();

        Documents = docs.Select(d => new DocumentViewModel
        {
            Id = d.Id,
            Number = d.Number,
            CreatedAt = d.CreatedAt,
            Status = d.Status,
            Warehouse = d.Warehouse.Name,
            Contractor = d.Contractors?.Name,
            TotalAmount = d.TotalAmount,
            ItemsCount = d.Items.Count,
            CreatedBy = d.CreatedByUser.Login
        }).ToList();
    }

  

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var doc = await _context.Documents.FindAsync(id);
        if (doc != null)
        {
            _context.Documents.Remove(doc);
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}