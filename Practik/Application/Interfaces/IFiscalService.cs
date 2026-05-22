using Praktik.Application.DTO;
namespace Praktik.Application.Interfaces;
public interface IFiscalService
{
    Task<(bool Success, string? FiscalNumber)> PrintReceiptAsync(List<CartItemDto> items, int warehouseId);
}