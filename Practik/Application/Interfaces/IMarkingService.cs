namespace Praktik.Application.Interfaces;
public interface IMarkingService
{
    Task<(bool IsValid, string? ProductInfo)> VerifyCodeAsync(string code);
    Task<bool> ReportSaleAsync(string code, string fiscalNumber);
}