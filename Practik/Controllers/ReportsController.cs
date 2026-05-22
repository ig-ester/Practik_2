using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Praktik.Infrastructure.Data;

namespace Praktik.Controllers;

[Route("api/reports")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("stock-history")]
    public async Task<IActionResult> GetStockHistory([FromQuery] int productId, [FromQuery] string? series, [FromQuery] string? warehouse)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound(new { error = "Товар не найден" });

            var warehouseName = warehouse ?? "Не указан";
            var seriesNumber = series ?? "Без серии";

            // Получаем историю движения по документам
            var documents = new List<object>();

            // Приходные документы (Receiving)
            var receivingDocs = await _context.DocumentItems
                .Include(di => di.Document)
                    .ThenInclude(d => d.Contractors)
                .Include(di => di.Batch)
                .Where(di => di.ProductId == productId && 
                             di.Document.Type == "Приход" &&
                             (series == null || di.Batch.SeriesNumber == series))
                .Select(di => new
                {
                    date = di.Document.CreatedAt,
                    number = di.Document.Number,
                    type = di.Document.Type,
                    quantity = di.Quantity,
                    contractor = di.Document.Contractors != null ? di.Document.Contractors.Name : null
                })
                .ToListAsync();

            documents.AddRange(receivingDocs);

            // Расходные документы (Sales, WriteOff)
            var salesDocs = await _context.DocumentItems
                .Include(di => di.Document)
                    .ThenInclude(d => d.Contractors)
                .Include(di => di.Batch)
                .Where(di => di.ProductId == productId &&
                             (di.Document.Type == "Продажа" || di.Document.Type == "Списание") &&
                             (series == null || di.Batch.SeriesNumber == series))
                .Select(di => new
                {
                    date = di.Document.CreatedAt,
                    number = di.Document.Number,
                    type = di.Document.Type,
                    quantity = -di.Quantity, // Расход показываем с минусом
                    contractor = di.Document.Contractors != null ? di.Document.Contractors.Name : null
                })
                .ToListAsync();

            documents.AddRange(salesDocs);

            // Сортируем по дате
            var sortedDocs = documents.OrderBy(d => 
            {
                var prop = d.GetType().GetProperty("date");
                return (DateTime?)prop.GetValue(d);
            }).ToList();

            return Ok(new
            {
                productName = product.Name,
                series = seriesNumber,
                warehouse = warehouseName,
                documents = sortedDocs
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
