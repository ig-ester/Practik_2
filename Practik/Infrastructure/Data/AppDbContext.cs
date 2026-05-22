using Microsoft.EntityFrameworkCore;
using Praktik.Domain.Entities;

namespace Praktik.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<Contractor> Contractors { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Batch> Batches { get; set; }
    public DbSet<MarkingCode> MarkingCodes { get; set; }
    public DbSet<Stock> Stock { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentItem> DocumentItems { get; set; }
    public DbSet<DocumentLog> DocumentLogs { get; set; }
    public DbSet<DocumentSequence> DocumentSequences { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);


        mb.Entity<User>().HasOne(u => u.Role).WithMany().HasForeignKey(u => u.RoleId);
        mb.Entity<User>().HasOne(u => u.Organization).WithMany().HasForeignKey(u => u.OrganizationId);

        mb.Entity<Batch>().HasOne(b => b.Product).WithMany().HasForeignKey(b => b.ProductId);
        mb.Entity<MarkingCode>().HasOne(m => m.Product).WithMany().HasForeignKey(m => m.ProductId);
        mb.Entity<MarkingCode>().HasOne(m => m.Batch).WithMany().HasForeignKey(m => m.BatchId);

        mb.Entity<Stock>().HasIndex(s => new { s.WarehouseId, s.BatchId }).IsUnique();
        mb.Entity<Stock>().HasOne(s => s.Warehouse).WithMany().HasForeignKey(s => s.WarehouseId);
        mb.Entity<Stock>().HasOne(s => s.Batch).WithMany().HasForeignKey(s => s.BatchId);

        mb.Entity<Document>().HasOne(d => d.Warehouse).WithMany().HasForeignKey(d => d.WarehouseId);
        mb.Entity<Document>().HasOne(d => d.Contractors).WithMany().HasForeignKey(d => d.ContractorId);
        mb.Entity<Document>().HasOne(d => d.CreatedByUser).WithMany().HasForeignKey(d => d.CreatedBy);

        mb.Entity<DocumentItem>().HasOne(di => di.Document).WithMany(d => d.Items).HasForeignKey(di => di.DocumentId);
        mb.Entity<DocumentItem>().HasOne(di => di.Product).WithMany().HasForeignKey(di => di.ProductId);
        mb.Entity<DocumentItem>()
        .HasOne(di => di.MarkingCodeEntity)
        .WithMany()
        .HasForeignKey(di => di.MarkingCode)
        .OnDelete(DeleteBehavior.Restrict)  
        .IsRequired(false);
        mb.Entity<DocumentItem>().HasOne(di => di.Batch).WithMany().HasForeignKey(di => di.BatchId);
        
        // Связь с рецептами
        mb.Entity<DocumentItem>().HasOne(di => di.Prescription).WithMany(p => p.DocumentItems).HasForeignKey(di => di.PrescriptionId).OnDelete(DeleteBehavior.SetNull);
        mb.Entity<Document>().HasOne(d => d.Prescription).WithMany().HasForeignKey(d => d.PrescriptionId).OnDelete(DeleteBehavior.SetNull);

        mb.Entity<DocumentLog>().HasOne(dl => dl.Document).WithMany(d => d.Logs).HasForeignKey(dl => dl.DocumentId);
    }
}