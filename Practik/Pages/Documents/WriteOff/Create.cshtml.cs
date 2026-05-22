using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Praktik.Application.DTO;
using Praktik.Application.Interfaces;
using Praktik.Application.Services;
using Praktik.Domain.Entities;
using Praktik.Domain.Enums;
using Praktik.Infrastructure.Data;

namespace Praktik.Pages.Documents.WriteOff;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly IDocumentService _docService;
    private readonly DocumentNumberService _numberService;

    public CreateModel(AppDbContext context, IDocumentService docService, DocumentNumberService numberService)
    {
        _context = context;
        _docService = docService;
        _numberService = numberService;
    }

    [BindProperty] public int WarehouseId { get; set; }
    [BindProperty] public string Reason { get; set; } = string.Empty; // Причина списания
    [BindProperty] public List<DocumentItemDto> Items { get; set; } = new();

    public IList<Warehouse> Warehouses { get; set; }
    public IList<Product> Products { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Warehouses = await _context.Warehouses.ToListAsync();
        Products = await _context.Products.ToListAsync();
        Items.Add(new DocumentItemDto());
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Reason))
        {
            ModelState.AddModelError("Reason", "Укажите причину списания");
            return Page();
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var doc = new Document
        {
            Number = await _numberService.GetNextNumberAsync("WriteOff"),
            Type = "WriteOff",
            Status = "Final",
            WarehouseId = WarehouseId,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            Comment = $"Причина: {Reason}"
        };

        var validItems = Items.Where(i => i.ProductId > 0 && i.Quantity > 0).ToList();
        if (!validItems.Any())
        {
            ModelState.AddModelError("", "Добавьте товары для списания");
            return Page();
        }

        await _docService.CreateDocumentAsync(doc, validItems);
        return RedirectToPage("/Documents/Index");
    }
    public JsonResult OnGetBatchesByProduct(int productId)
    {
        try
        {
            var batches = _context.Batches
                .Where(b => b.ProductId == productId)
                .Select(b => new
                {
                    id = b.Id,
                    seriesNumber = b.SeriesNumber,
                    expirationDate = b.ExpirationDate
                })
                .ToList();

            return new JsonResult(batches);
        }
        catch (Exception ex)
        {
            Response.StatusCode = 500;
            return new JsonResult(new { error = ex.Message });
        }
    }
}