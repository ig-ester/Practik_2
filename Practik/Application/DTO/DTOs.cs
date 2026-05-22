namespace Praktik.Application.DTO;

public class LoginDto { public string Login { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
public class CartItemDto { public int ProductId { get; set; } public int? BatchId { get; set; } public string? MarkingCode { get; set; } public int Quantity { get; set; } public decimal Price { get; set; } }
public class DocumentItemDto { public int ProductId { get; set; } public int? BatchId { get; set; } public string? MarkingCode { get; set; } public int Quantity { get; set; } public decimal Price { get; set; } }
public class StockFilterDto { public int? WarehouseId { get; set; } public int? OrganizationId { get; set; } public string? Category { get; set; } public DateTime? ExpirationBefore { get; set; } }
public class StockReportDto 
{ 
    public int ProductId { get; set; }
    public string Warehouse { get; set; } = string.Empty; 
    public string ProductName { get; set; } = string.Empty; 
    public string? Category { get; set; } 
    public string? Series { get; set; } 
    public DateTime? ExpirationDate { get; set; } 
    public int Quantity { get; set; } 
    public decimal Price { get; set; } 
    public decimal TotalValue { get; set; } 
    public bool IsExpiringSoon { get; set; } 
}