using Praktik.Application.DTO;
namespace Praktik.Application.Interfaces;
public interface ISalesService
{
    Task<(bool Success, string? ReceiptNumber)> ProcessRetailSaleAsync(int warehouseId, List<CartItemDto> cart, int userId);
}