using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Praktik.Domain.Entities;

[Table("Users")]
public class User
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(100)] public string Login { get; set; } = string.Empty;
    [Required, MaxLength(255)] public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}