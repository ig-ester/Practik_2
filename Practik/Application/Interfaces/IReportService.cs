using Praktik.Application.DTO;
namespace Praktik.Application.Interfaces;
public interface IReportService
{
    Task<List<StockReportDto>> GetStockReportAsync(StockFilterDto filter);
    Task<byte[]> ExportToExcelAsync(List<StockReportDto> data);
}