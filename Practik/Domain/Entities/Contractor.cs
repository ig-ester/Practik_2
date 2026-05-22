using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Praktik.Domain.Entities;

[Table("Contractors")]
public class Contractor
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
    [MaxLength(20)] public string Type { get; set; } = "Supplier";
    [MaxLength(12)] public string? INN { get; set; }
    [MaxLength(255)] public string? Address { get; set; }
    [MaxLength(255)] public string? ContactInfo { get; set; }
}