using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Praktik.Application.DTO;
using Praktik.Application.Interfaces;
using Praktik.Application.Services;
using Praktik.Domain.Entities;
using Praktik.Domain.Enums;
using Praktik.Infrastructure.Data;

namespace Praktik.Pages.Documents.Sales;

public class WholesaleModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly IDocumentService _docService;
    private readonly DocumentNumberService _numberService;

    public WholesaleModel(AppDbContext context, IDocumentService docService, DocumentNumberService numberService)
    {
        _context = context;
        _docService = docService;
        _numberService = numberService;
    }

    [BindProperty] public int WarehouseId { get; set; }
    [BindProperty] public int BuyerId { get; set; }
    [BindProperty] public List<DocumentItemDto> Items { get; set; } = new();
    [BindProperty] public int? PrescriptionId { get; set; }

    public IList<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
    public IList<Contractor> Buyers { get; set; } = new List<Contractor>();
    public IList<Product> Products { get; set; } = new List<Product>();

    public async Task<IActionResult> OnGetAsync()
    {
        Warehouses = await _context.Warehouses.ToListAsync();
        Buyers = await _context.Contractors.Where(c => c.Type == "Buyer").ToListAsync();
        Products = await _context.Products.OrderBy(p => p.Name).ToListAsync();
        Items.Add(new DocumentItemDto());
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid || BuyerId == 0 || WarehouseId == 0)
            return Page();

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        var doc = new Document
        {
            Number = await _numberService.GetNextNumberAsync("WholesaleSale"),
            Type = "WholesaleSale",
            Status = "Final",
            WarehouseId = WarehouseId,
            ContractorId = BuyerId,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            PrescriptionId = PrescriptionId
        };

        var validItems = Items.Where(i => i.ProductId > 0 && i.BatchId.HasValue && i.Quantity > 0).ToList();
        if (!validItems.Any())
        {
            ModelState.AddModelError("", "Добавьте товары с указанием серии");
            return Page();
        }

        await _docService.CreateDocumentAsync(doc, validItems);
        TempData["Success"] = "Оптовая продажа успешно оформлена";
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