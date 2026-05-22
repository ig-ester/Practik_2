using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Praktik.Application.DTO;

using Praktik.Application.Interfaces;

namespace Praktik.Pages.Reports;

public class StockModel : PageModel
{
    private readonly IReportService _report;
    public StockModel(IReportService report) => _report = report;

    public List<StockReportDto> Reports { get; set; } = new();
    public StockFilterDto Filter { get; set; } = new();

    public async Task OnGetAsync(StockFilterDto filter)
    {
        Filter = filter;
        Reports = await _report.GetStockReportAsync(Filter);
    }

    public async Task<IActionResult> OnGetExportAsync(StockFilterDto filter)
    {
        var data = await _report.GetStockReportAsync(filter);
        var bytes = await _report.ExportToExcelAsync(data);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Stock_Report.xlsx");
    }
}