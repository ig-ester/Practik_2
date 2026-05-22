using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Praktik.Domain.Enums;
using Praktik.Infrastructure.Data;

namespace Praktik.Pages.Documents;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly AppDbContext _context;

    public DetailsModel(AppDbContext context)
    {
        _context = context;
    }

    public DocumentViewModel Document { get; set; } = null!;
    public List<DocumentItemViewModel> Items { get; set; } = new();

    public class DocumentViewModel
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string? ContractorName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Comment { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
    }

    public class DocumentItemViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? SeriesNumber { get; set; }
        public string? MarkingCode { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Sum { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var doc = await _context.Documents
            .Include(d => d.Warehouse)
            .Include(d => d.Contractors)
            .Include(d => d.CreatedByUser)
            .Include(d => d.Items).ThenInclude(i => i.Product)
            .Include(d => d.Items).ThenInclude(i => i.Batch)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doc == null)
            return NotFound();

        Document = new DocumentViewModel
        {
            Id = doc.Id,
            Number = doc.Number,
            Type = doc.Type,
            Status = doc.Status,
            WarehouseName = doc.Warehouse.Name,
            ContractorName = doc.Contractors?.Name,
            TotalAmount = doc.TotalAmount,
            CreatedAt = doc.CreatedAt,
            CompletedAt = doc.CompletedAt,
            Comment = doc.Comment,
            CreatedByName = doc.CreatedByUser.Login
        };

        Items = doc.Items.Select(i => new DocumentItemViewModel
        {
            Id = i.Id,
            ProductName = i.Product.Name,
            SeriesNumber = i.Batch?.SeriesNumber,
            MarkingCode = i.MarkingCode,
            Quantity = i.Quantity,
            Price = i.Price,
            Sum = i.Sum
        }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostChangeStatusAsync(int id, string status)
    {
        var doc = await _context.Documents.FindAsync(id);
        if (doc == null)
            return NotFound();

        if (doc.Status == "Final")
        {
            TempData["Error"] = "Проведённый документ нельзя изменить";
            return RedirectToPage("Details", new { id });
        }

        if (Enum.TryParse<DocumentStatus>(status, true, out var newStatus))
        {
            doc.Status = newStatus.ToString();
            if (newStatus == DocumentStatus.Final)
                doc.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Статус изменён на {newStatus}";
        }

        return RedirectToPage("Details", new { id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var doc = await _context.Documents
            .Include(d => d.Items)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doc == null)
            return NotFound();

        if (doc.Status == "Final")
        {
            TempData["Error"] = "Проведённый документ нельзя удалить";
            return RedirectToPage("Details", new { id });
        }

        _context.DocumentItems.RemoveRange(doc.Items);
        _context.Documents.Remove(doc);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Документ удалён";
        return RedirectToPage("/Documents/Index");
    }
}