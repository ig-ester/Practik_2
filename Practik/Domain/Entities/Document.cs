using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Praktik.Domain.Enums;

namespace Praktik.Domain.Entities;

[Table("Documents")]
public class Document
{
    [Key] public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Number { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Type { get; set; } = string.Empty;
 
    [MaxLength(20)]
    public string Status { get; set; } = "Draft";

    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public int? ContractorId { get; set; }
    public Contractor? Contractors { get; set; }

    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }

    public ICollection<DocumentItem> Items { get; set; } = new List<DocumentItem>();
    public ICollection<DocumentLog> Logs { get; set; } = new List<DocumentLog>();

    // Связь с рецептом (для продаж товаров по рецепту)
    public int? PrescriptionId { get; set; }
    public Prescription? Prescription { get; set; }

    [NotMapped]
    public DocumentType DocumentType
    {
        get => Enum.Parse<DocumentType>(Type);
        set => Type = value.ToString();
    }

    [NotMapped]
    public DocumentStatus DocumentStatus
    {
        get => Enum.Parse<DocumentStatus>(Status);
        set => Status = value.ToString();
    }
}