using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Praktik.Domain.Entities;

[Table("DocumentSequences")]
public class DocumentSequence
{
    [Key]
    [MaxLength(50)]
    public string DocumentType { get; set; } = string.Empty;

    public long CurrentNumber { get; set; } = 0;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}