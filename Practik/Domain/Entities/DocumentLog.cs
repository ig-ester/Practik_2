using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Praktik.Domain.Entities;

[Table("DocumentLogs")]
public class DocumentLog
{
    [Key] public int Id { get; set; }
    public int DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    [MaxLength(20)] public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int UserId { get; set; }
    [MaxLength(2000)] public string? Details { get; set; }
}