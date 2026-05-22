using Practik.Infrastructure.DTOs;

namespace Practik.Application.Interfaces;

public interface IStockService
{
    Task<List<StockReportDto>> GetStockReportAsync(StockFilterDto filter);
    Task<byte[]> ExportToExcelAsync(List<StockReportDto> data);
}