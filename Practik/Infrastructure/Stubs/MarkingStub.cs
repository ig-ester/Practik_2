using Praktik.Application.Interfaces;

namespace Praktik.Infrastructure.Stubs;

public class MarkingStub : IMarkingService
{
    private static readonly Random _r = new();

    public async Task<(bool IsValid, string? ProductInfo)> VerifyCodeAsync(string code)
    {
        await Task.Delay(_r.Next(1500, 2001));
        if (string.IsNullOrWhiteSpace(code) || code.Length < 10) return (false, null);
        return (true, $"Товар: Препарат #{code.Substring(code.Length - 4)}");
    }

    public async Task<bool> ReportSaleAsync(string code, string fiscalNumber)
    {
        await Task.Delay(300);
        return true;
    }
}