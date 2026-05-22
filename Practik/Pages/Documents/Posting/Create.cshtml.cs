using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Praktik.Application.DTO;
using Praktik.Application.Interfaces;
using Praktik.Application.Services;
using Praktik.Domain.Entities;
using Praktik.Infrastructure.Data;

namespace Praktik.Pages.Documents.Posting;

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
    [BindProperty] public int? ContractorId { get; set; }
    [BindProperty] public string? Comment { get; set; }
    [BindProperty] public List<PostingItemDto> Items { get; set; } = new();

    public IList<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
    public IList<Contractor> Contractors { get; set; } = new List<Contractor>();
    public IList<Product> Products { get; set; } = new List<Product>();

    public class PostingItemDto
    {
        public int ProductId { get; set; }
        public int? BatchId { get; set; }
        public string? NewBatchSeries { get; set; }
        public DateTime? BatchExpiration { get; set; }
        public string? MarkingCode { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public IActionResult OnGet()
    {
        Warehouses = _context.Warehouses.OrderBy(w => w.Name).ToList();
        Contractors = _context.Contractors.OrderBy(c => c.Name).ToList();
        Products = _context.Products.OrderBy(p => p.Name).ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (WarehouseId == 0)
        {
            ModelState.AddModelError("WarehouseId", "Выберите склад");
        }

        if (!ModelState.IsValid || !Items.Any())
        {
            ModelState.AddModelError("", "Добавьте хотя бы одну позицию товара");
            Warehouses = _context.Warehouses.OrderBy(w => w.Name).ToList();
            Contractors = _context.Contractors.OrderBy(c => c.Name).ToList();
            Products = _context.Products.OrderBy(p => p.Name).ToList();
            return Page();
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        var doc = new Document
        {
            Number = await _numberService.GetNextNumberAsync("Posting"),
            Type = "Posting",
            Status = "Final",
            WarehouseId = WarehouseId,
            ContractorId = ContractorId,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            Comment = Comment
        };

        var docItems = new List<DocumentItemDto>();
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in Items.Where(i => i.ProductId > 0 && i.Quantity > 0))
            {
                int batchId = item.BatchId ?? 0;

                if (!item.BatchId.HasValue && !string.IsNullOrEmpty(item.NewBatchSeries) && item.BatchExpiration.HasValue)
                {
                    if (item.BatchExpiration.Value.Date <= DateTime.Now.Date)
                    {
                        ModelState.AddModelError("", $"Ошибка: Срок годности для серии {item.NewBatchSeries} не может быть в прошлом или сегодня");
                        await transaction.RollbackAsync();
                        Warehouses = _context.Warehouses.OrderBy(w => w.Name).ToList();
                        Contractors = _context.Contractors.OrderBy(c => c.Name).ToList();
                        Products = _context.Products.OrderBy(p => p.Name).ToList();
                        return Page();
                    }

                    var existingBatch = await _context.Batches
                        .FirstOrDefaultAsync(b => b.ProductId == item.ProductId && b.SeriesNumber == item.NewBatchSeries);

                    if (existingBatch != null)
                    {
                        batchId = existingBatch.Id;
                    }
                    else
                    {
                        var newBatch = new Batch
                        {
                            ProductId = item.ProductId,
                            SeriesNumber = item.NewBatchSeries,
                            ExpirationDate = item.BatchExpiration.Value,
                            ManufactureDate = DateTime.Now.AddYears(-2)
                        };
                        _context.Batches.Add(newBatch);
                        await _context.SaveChangesAsync();
                        batchId = newBatch.Id;
                    }
                }

                if (batchId == 0)
                {
                    ModelState.AddModelError("", $"Для товара {item.ProductId} не указана серия");
                    await transaction.RollbackAsync();
                    Warehouses = _context.Warehouses.OrderBy(w => w.Name).ToList();
                    Contractors = _context.Contractors.OrderBy(c => c.Name).ToList();
                    Products = _context.Products.OrderBy(p => p.Name).ToList();
                    return Page();
                }

                docItems.Add(new DocumentItemDto
                {
                    ProductId = item.ProductId,
                    BatchId = batchId,
                    MarkingCode = item.MarkingCode,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            if (!docItems.Any())
            {
                ModelState.AddModelError("", "Добавьте хотя бы одну позицию");
                await transaction.RollbackAsync();
                Warehouses = _context.Warehouses.OrderBy(w => w.Name).ToList();
                Contractors = _context.Contractors.OrderBy(c => c.Name).ToList();
                Products = _context.Products.OrderBy(p => p.Name).ToList();
                return Page();
            }

            await _docService.CreateDocumentAsync(doc, docItems);
            await transaction.CommitAsync();

            TempData["Success"] = $"Оприходование {doc.Number} успешно проведено";
            return RedirectToPage("/Documents/Posting/Index");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError("", $"Ошибка сохранения: {ex.Message}");
            Warehouses = _context.Warehouses.OrderBy(w => w.Name).ToList();
            Contractors = _context.Contractors.OrderBy(c => c.Name).ToList();
            Products = _context.Products.OrderBy(p => p.Name).ToList();
            return Page();
        }
    }

    public JsonResult OnGetBatchesByProduct(int productId)
    {
        var batches = _context.Batches
            .Where(b => b.ProductId == productId)
            .Select(b => new { id = b.Id, seriesNumber = b.SeriesNumber, expirationDate = b.ExpirationDate })
            .ToList();
        return new JsonResult(batches);
    }
}