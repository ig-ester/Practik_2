using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Praktik.Domain.Entities;

[Table("Stock")]
public class Stock
{
    [Key] public int Id { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public int BatchId { get; set; }
    public Batch Batch { get; set; } = null!;
    public int Quantity { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}