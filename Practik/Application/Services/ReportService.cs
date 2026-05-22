using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Praktik.Application.Interfaces;
using Praktik.Infrastructure.Data;
using Praktik.Application.DTO;

namespace Praktik.Application.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;
    public ReportService(AppDbContext db) => _db = db;

    public async Task<List<StockReportDto>> GetStockReportAsync(StockFilterDto f)
    {
        var q = _db.Stock.Include(s => s.Batch).ThenInclude(b => b.Product).Include(s => s.Warehouse).AsQueryable();
        if (f.WarehouseId.HasValue) q = q.Where(s => s.WarehouseId == f.WarehouseId);
        if (f.OrganizationId.HasValue) q = q.Where(s => s.Warehouse.OrganizationId == f.OrganizationId);
        if (!string.IsNullOrEmpty(f.Category)) q = q.Where(s => s.Batch.Product.Category == f.Category);
        if (f.ExpirationBefore.HasValue) q = q.Where(s => s.Batch.ExpirationDate <= f.ExpirationBefore);

        return await q.Select(s => new StockReportDto
        {
            Warehouse = s.Warehouse.Name,
            ProductName = s.Batch.Product.Name,
            Category = s.Batch.Product.Category,
            Series = s.Batch.SeriesNumber,
            ExpirationDate = s.Batch.ExpirationDate,
            Quantity = s.Quantity,
            Price = s.Batch.Product.PriceRetail,
            TotalValue = s.Quantity * s.Batch.Product.PriceRetail,
            IsExpiringSoon = s.Batch.ExpirationDate <= DateTime.UtcNow.AddDays(30)
        }).ToListAsync();
    }

    public async Task<byte[]> ExportToExcelAsync(List<StockReportDto> data)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Остатки");
        var headers = new[] { "Склад", "Товар", "Категория", "Серия", "Годен до", "Кол-во", "Цена", "Сумма", "Скоро истекает" };
        for (int i = 0; i < headers.Length; i++) ws.Cell(1, i + 1).Value = headers[i];
        ws.Row(1).Style.Font.Bold = true;

        for (int r = 0; r < data.Count; r++)
        {
            var d = data[r]; int row = r + 2;
            ws.Cell(row, 1).Value = d.Warehouse; ws.Cell(row, 2).Value = d.ProductName; ws.Cell(row, 3).Value = d.Category ?? "-";
            ws.Cell(row, 4).Value = d.Series ?? "-"; ws.Cell(row, 5).Value = d.ExpirationDate?.ToString("dd.MM.yyyy") ?? "-";
            ws.Cell(row, 6).Value = d.Quantity; ws.Cell(row, 7).Value = d.Price; ws.Cell(row, 8).Value = d.TotalValue;
            ws.Cell(row, 9).Value = d.IsExpiringSoon ? "Да" : "Нет";
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}