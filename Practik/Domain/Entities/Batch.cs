using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Praktik.Domain.Entities;

[Table("Batches")]
public class Batch
{
    [Key] public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    [MaxLength(50)] public string SeriesNumber { get; set; } = string.Empty;
    [MaxLength(14)] public string? Gtin { get; set; }
    public DateTime? ManufactureDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
}