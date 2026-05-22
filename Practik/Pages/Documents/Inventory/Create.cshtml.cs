using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Praktik.Application.DTO;
using Praktik.Application.Interfaces;
using Praktik.Application.Services;
using Praktik.Domain.Entities;
using Praktik.Domain.Enums;
using Praktik.Infrastructure.Data;

namespace Praktik.Pages.Documents.Inventory;

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
    [BindProperty] public DateTime InventoryDate { get; set; } = DateTime.Now;
    [BindProperty] public string? Comment { get; set; }
    [BindProperty] public List<InventoryItemDto> Items { get; set; } = new();

    public IList<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
    public IList<Product> Products { get; set; } = new List<Product>();
    public IList<Batch> Batches { get; set; } = new List<Batch>();

    public class InventoryItemDto
    {
        public int ProductId { get; set; }
        public int BatchId { get; set; }
        public string? MarkingCode { get; set; }
        public int ExpectedQty { get; set; } 
        public int ActualQty { get; set; }  
    }

    public async Task<IActionResult> OnGetAsync()
    {
        Warehouses = await _context.Warehouses.OrderBy(w => w.Name).ToListAsync();
        Products = await _context.Products.OrderBy(p => p.Name).ToListAsync();
        Batches = await _context.Batches.Include(b => b.Product).OrderBy(b => b.SeriesNumber).ToListAsync();

        Items.Add(new InventoryItemDto()); 
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid || WarehouseId == 0) return Page();

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        var doc = new Document
        {
            Number = await _numberService.GetNextNumberAsync("Inventory"),
            Type = "Inventory",
            Status = "Final",
            WarehouseId = WarehouseId,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            Comment = Comment
        };

        var docItems = Items
            .Where(i => i.ProductId > 0 && i.BatchId > 0)
            .Select(i => new DocumentItemDto
            {
                ProductId = i.ProductId,
                BatchId = i.BatchId,
                MarkingCode = i.MarkingCode,
                Quantity = i.ActualQty, 
                Price = 0 
            }).ToList();

        if (!docItems.Any())
        {
            ModelState.AddModelError("", "Добавьте хотя бы одну позицию");
            return Page();
        }

        await _docService.CreateDocumentAsync(doc, docItems);

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

    public async Task<JsonResult> OnGetExpectedStockAsync(int warehouseId, int batchId)
    {
        var stock = await _context.Stock
            .FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.BatchId == batchId);

        return new JsonResult(new { quantity = stock?.Quantity ?? 0 });
    }
    public async Task<JsonResult> OnGetStockByDateAsync(int warehouseId, string date)
    {
        try
        {
            if (!DateTime.TryParse(date, out var targetDate))
                return new JsonResult(new { error = "Некорректная дата" }) { StatusCode = 400 };

            var stockData = await _context.Stock
                .Include(s => s.Batch).ThenInclude(b => b.Product)
                .Where(s => s.WarehouseId == warehouseId && s.Quantity > 0)
                .Select(s => new
                {
                    productId = s.Batch.ProductId,
                    productName = s.Batch.Product.Name,
                    batchId = s.Batch.Id,
                    seriesNumber = s.Batch.SeriesNumber,
                    quantity = s.Quantity
                })
                .ToListAsync();

            return new JsonResult(stockData);
        }
        catch (Exception ex)
        {
            Response.StatusCode = 500;
            return new JsonResult(new { error = ex.Message });
        }
    }
}
