using Microsoft.EntityFrameworkCore;
using Praktik.Application.DTO;
using Praktik.Application.Interfaces;
using Praktik.Domain.Entities;
using Praktik.Domain.Enums;
using Praktik.Infrastructure.Data;

namespace Praktik.Application.Services;

public class SalesService : ISalesService
{
    private readonly AppDbContext _db;
    private readonly IMarkingService _marking;
    private readonly IFiscalService _fiscal;
    private readonly DocumentNumberService _numberService;

    public SalesService(AppDbContext db, IMarkingService marking, IFiscalService fiscal, DocumentNumberService numberService)
    {
        _db = db;
        _marking = marking;
        _fiscal = fiscal;
        _numberService = numberService;
    }

    public async Task<(bool Success, string? ReceiptNumber)> ProcessRetailSaleAsync(int warehouseId, List<CartItemDto> cart, int userId)
    {
        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var doc = new Document
            {
                Number = await _numberService.GetNextNumberAsync("RetailSale"), 
                Type = "RetailSale",
                Status = "Final",
                WarehouseId = warehouseId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };

            foreach (var item in cart)
            {
                var stock = await _db.Stock.FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.BatchId == item.BatchId);
                if (stock == null || stock.Quantity < item.Quantity)
                    throw new InvalidOperationException("Недостаточно товара");

                if (!string.IsNullOrEmpty(item.MarkingCode))
                {
                    var (valid, _) = await _marking.VerifyCodeAsync(item.MarkingCode);
                    if (!valid) throw new InvalidOperationException($"Код {item.MarkingCode} недействителен");
                }

                _db.DocumentItems.Add(new DocumentItem
                {
                    Document = doc,
                    ProductId = item.ProductId,
                    BatchId = item.BatchId,
                    MarkingCode = item.MarkingCode,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Sum = item.Price * item.Quantity
                });
                stock.Quantity -= item.Quantity;
            }

            doc.TotalAmount = cart.Sum(i => i.Price * i.Quantity);
            _db.Documents.Add(doc);

            var (fOk, fNum) = await _fiscal.PrintReceiptAsync(cart, warehouseId);
            if (!fOk) throw new Exception("Ошибка ККМ");

            foreach (var i in cart.Where(c => !string.IsNullOrEmpty(c.MarkingCode)))
                await _marking.ReportSaleAsync(i.MarkingCode!, fNum!);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return (true, fNum);
        }
        catch { await tx.RollbackAsync(); throw; }
    }
}