using Microsoft.EntityFrameworkCore;
using Praktik.Domain.Entities;
using Praktik.Infrastructure.Data;

namespace Praktik.Application.Services;

public class DocumentNumberService
{
    private readonly AppDbContext _context;
    private static readonly object _lock = new object();

    public DocumentNumberService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> GetNextNumberAsync(string documentType)
    {
        char prefix = documentType[0];

        using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        try
        {
            var sequence = await _context.DocumentSequences
                .FirstOrDefaultAsync(s => s.DocumentType == documentType);

            if (sequence == null)
            {
                sequence = new DocumentSequence
                {
                    DocumentType = documentType,
                    CurrentNumber = 0,
                    LastUpdated = DateTime.UtcNow
                };
                _context.DocumentSequences.Add(sequence);
                await _context.SaveChangesAsync();
            }

            sequence.CurrentNumber++;
            sequence.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();


            string number = sequence.CurrentNumber.ToString("00000000000000");
            string docNumber = $"{prefix}-{number}";

            await transaction.CommitAsync();
            return docNumber;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}