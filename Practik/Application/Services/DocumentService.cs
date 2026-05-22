using Microsoft.EntityFrameworkCore;
using Praktik.Application.DTO;
using Praktik.Application.Interfaces;
using Praktik.Domain.Entities;
using Praktik.Domain.Enums;
using Praktik.Infrastructure.Data;

namespace Praktik.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly AppDbContext _db;

    public DocumentService(AppDbContext db) => _db = db;

    public async Task<Document> CreateDocumentAsync(Document doc, List<DocumentItemDto> items)
    {
        try
        {
            _db.Documents.Add(doc);
            decimal total = 0;

            foreach (var i in items)
            {
 
                if (!string.IsNullOrEmpty(i.MarkingCode))
                {
                    var markingExists = await _db.MarkingCodes
                        .AnyAsync(mc => mc.Code == i.MarkingCode);

                    if (!markingExists)
                    {
                        var newMarking = new MarkingCode
                        {
                            Code = i.MarkingCode,
                            ProductId = i.ProductId,
                            BatchId = i.BatchId ?? 0,
                            Status = "Active",
                            ReceivedAt = DateTime.UtcNow
                        };
                        _db.MarkingCodes.Add(newMarking);
                        await _db.SaveChangesAsync();
                    }
                }

                var docItem = new DocumentItem
                {
                    Document = doc,
                    ProductId = i.ProductId,
                    BatchId = i.BatchId,
                    MarkingCode = i.MarkingCode,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    Sum = i.Quantity * i.Price
                };
                _db.DocumentItems.Add(docItem);
                total += docItem.Sum;

                if (i.BatchId.HasValue)
                {
                    var stock = await _db.Stock
                        .FirstOrDefaultAsync(s => s.WarehouseId == doc.WarehouseId && s.BatchId == i.BatchId.Value);

                    if (stock == null)
                    {
                        _db.Stock.Add(new Stock
                        {
                            WarehouseId = doc.WarehouseId,
                            BatchId = i.BatchId.Value,
                            Quantity = i.Quantity,
                            LastUpdated = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        stock.Quantity += i.Quantity;
                        stock.LastUpdated = DateTime.UtcNow;
                    }
                }
            }

            doc.TotalAmount = total;
            doc.Status = "Final"; 
            doc.CompletedAt = DateTime.UtcNow;
            _db.DocumentLogs.Add(new DocumentLog { Document = doc, Action = "Created", UserId = doc.CreatedBy });

            await _db.SaveChangesAsync();
            return doc;
        }
        catch
        {
            throw;
        }
    }
}