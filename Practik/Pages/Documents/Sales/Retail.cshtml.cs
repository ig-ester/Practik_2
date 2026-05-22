using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Praktik.Application.Services;
using Praktik.Application.Interfaces;
using Praktik.Domain.Entities;
using Praktik.Infrastructure.Data;
using System.Text.Json;

namespace Praktik.Pages.Documents.Sales;
public class RetailModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly SalesService _salesService;
    private readonly IMarkingService _markingService;

    public RetailModel(AppDbContext context, SalesService sales, IMarkingService marking)
    {
        _context = context;
        _salesService = sales;
        _markingService = marking;
    }

    public List<CartItem> Cart { get; set; } = new();

    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int? BatchId { get; set; }
        public string? MarkingCode { get; set; }
        public decimal Price { get; set; }
    }

    public IList<Product> Products { get; set; } = new List<Product>();

    public IActionResult OnGet()
    {
        Products = _context.Products.OrderBy(p => p.Name).ToList();
        var json = TempData["Cart"] as string;
        if (!string.IsNullOrEmpty(json))
            Cart = JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        return Page();
    }

    public async Task<JsonResult> OnGetScanAsync(string code)
    {
        var (isValid, _) = await _markingService.VerifyCodeAsync(code);
        if (!isValid) return new JsonResult(new { error = "Код недействителен" });

        var item = new CartItem { ProductId = 1, ProductName = "Товар " + code.Substring(0, 4), BatchId = 1, MarkingCode = code, Price = 100 };
        Cart.Add(item);
        TempData["Cart"] = JsonSerializer.Serialize(Cart);
        return new JsonResult(new { item });
    }

    public JsonResult OnPostAddProduct(int productId, int? batchId)
    {
        var product = _context.Products.Find(productId);
        if (product == null) return new JsonResult(new { error = "Товар не найден" });

        var item = new CartItem 
        { 
            ProductId = productId, 
            ProductName = product.Name, 
            BatchId = batchId, 
            MarkingCode = "", 
            Price = product.PriceRetail 
        };
        Cart.Add(item);
        TempData["Cart"] = JsonSerializer.Serialize(Cart);
        return new JsonResult(new { success = true });
    }

    public JsonResult OnPostRemoveItem(int productId)
    {
        var item = Cart.FirstOrDefault(c => c.ProductId == productId);
        if (item != null)
        {
            Cart.Remove(item);
            TempData["Cart"] = JsonSerializer.Serialize(Cart);
        }
        return new JsonResult(new { success = true });
    }

    public async Task<IActionResult> OnPostPayAsync(int warehouseId)
    {
        if (!Cart.Any()) return Page();
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        var saleItems = Cart.Select(c => new Application.DTO.CartItemDto
        {
            ProductId = c.ProductId,
            BatchId = c.BatchId,
            MarkingCode = c.MarkingCode,
            Quantity = 1,
            Price = c.Price
        }).ToList();

        var res = await _salesService.ProcessRetailSaleAsync(warehouseId, saleItems, userId);
        if (res.Success)
        {
            TempData["Success"] = $"Продажа проведена! Чек №{res.ReceiptNumber}";
            Cart.Clear();
            TempData["Cart"] = JsonSerializer.Serialize(Cart);
        }
        else
        {
            ModelState.AddModelError("", "Ошибка оплаты: " + res.ReceiptNumber);
        }
        return RedirectToPage();
    }
}