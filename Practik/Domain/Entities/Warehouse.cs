using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Praktik.Domain.Entities;

[Table("Warehouses")]
public class Warehouse
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(20)] public string Type { get; set; } = "Склад";
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
}