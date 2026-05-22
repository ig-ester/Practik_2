using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Praktik.Domain.Entities;

[Table("DocumentItems")]
public class DocumentItem
{
    [Key] public int Id { get; set; }
    public int DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int? BatchId { get; set; }
    public Batch? Batch { get; set; }

    [MaxLength(150)]
    public string? MarkingCode { get; set; }

    public MarkingCode? MarkingCodeEntity { get; set; }

    public int Quantity { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Price { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Sum { get; set; }
    
    // Связь с рецептом (для товаров требующих рецепт)
    public int? PrescriptionId { get; set; }
    public Prescription? Prescription { get; set; }
}