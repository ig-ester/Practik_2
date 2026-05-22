using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Praktik.Domain.Entities;

[Table("Products")]
public class Product
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
    [MaxLength(100)] public string? Category { get; set; }
    [MaxLength(20)] public string Unit { get; set; } = "шт";
    [Column(TypeName = "decimal(18,2)")] public decimal PriceRetail { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal PriceWholesale { get; set; }
    public bool IsMarked { get; set; } = false;
    [MaxLength(50)] public string? Barcode { get; set; }
    [MaxLength(14)] public string? Gtin { get; set; }
    public int CurrentStock { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Новые признаки для номенклатуры
    [MaxLength(50)] public string? MarkingCategory { get; set; }
    public bool IsPoison { get; set; } = false;
    public bool IsZnvlp { get; set; } = false;
    public bool IsPrescriptionOnly { get; set; } = false;
}