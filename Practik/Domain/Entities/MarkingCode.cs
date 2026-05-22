using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Praktik.Domain.Entities;

[Table("MarkingCodes")]
public class MarkingCode
{
    [Key, MaxLength(150)] public string Code { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int BatchId { get; set; }
    public Batch Batch { get; set; } = null!;
    [MaxLength(20)] public string Status { get; set; } = "Active";
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastCheckDate { get; set; }
}