using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Praktik.Domain.Entities;
using Praktik.Domain.Enums;
using Praktik.Infrastructure.Data;

namespace Praktik.Pages.Documents.Sales;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public IList<SaleDocumentViewModel> Documents { get; set; } = new List<SaleDocumentViewModel>();
    public string? SearchTerm { get; set; }
    public string? TypeFilter { get; set; }

    public class SaleDocumentViewModel
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Warehouse { get; set; } = string.Empty;
        public string? Contractor { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemsCount { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public async Task OnGetAsync(string? searchTerm, string? type)
    {
        SearchTerm = searchTerm;
        TypeFilter = type;

        var query = _context.Documents
            .Include(d => d.Warehouse)
            .Include(d => d.Contractors)
            .Include(d => d.CreatedByUser)
            .Include(d => d.Items)
            .Where(d => d.Type == "RetailSale" || d.Type == "WholesaleSale")
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(d => d.Number.Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(type))
        {
            var docType = Enum.Parse<DocumentType>(type);
            query = query.Where(d => d.Type == docType.ToString());
        }

        var docs = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();

        Documents = docs.Select(d => new SaleDocumentViewModel
        {
            Id = d.Id,
            Number = d.Number,
            CreatedAt = d.CreatedAt,
            Type = d.Type == "RetailSale" ? "Розница" : "Опт",
            Warehouse = d.Warehouse.Name,
            Contractor = d.Contractors?.Name,
            TotalAmount = d.TotalAmount,
            ItemsCount = d.Items.Count,
            CreatedBy = d.CreatedByUser.Login
        }).ToList();
    }
}