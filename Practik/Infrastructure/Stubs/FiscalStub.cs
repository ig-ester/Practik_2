using Praktik.Application.DTO;
using Praktik.Application.Interfaces;

namespace Praktik.Infrastructure.Stubs;

public class FiscalStub : IFiscalService
{
    private static readonly Random _r = new();
    public async Task<(bool Success, string? FiscalNumber)> PrintReceiptAsync(List<CartItemDto> items, int warehouseId)
    {
        await Task.Delay(500);
        return (true, $"FN-{DateTime.UtcNow:yyMMdd}-{_r.Next(10000, 99999)}");
    }
}