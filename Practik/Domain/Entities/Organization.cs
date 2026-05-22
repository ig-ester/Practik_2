using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Praktik.Domain.Entities;

[Table("Organizations")]
public class Organization
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(255)] public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}