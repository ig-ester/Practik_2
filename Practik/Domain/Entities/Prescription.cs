using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Praktik.Domain.Entities;

[Table("Prescriptions")]
public class Prescription
{
    [Key] public int Id { get; set; }
    
    [Required, MaxLength(50)]
    public string Number { get; set; } = string.Empty;
    
    public DateTime IssueDate { get; set; }
    
    [MaxLength(200)]
    public string? DoctorName { get; set; }
    
    [MaxLength(300)]
    public string? ClinicName { get; set; }
    
    [MaxLength(150)]
    public string? PatientName { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Навигационное свойство
    public ICollection<DocumentItem> DocumentItems { get; set; } = new List<DocumentItem>();
}
