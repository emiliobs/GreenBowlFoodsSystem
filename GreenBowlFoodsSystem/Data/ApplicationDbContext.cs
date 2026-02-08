using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GreenBowlFoodsSystem.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<RawMaterial> RawMaterials { get; set; }
    public DbSet<PackagingMaterial> PackagingMaterials { get; set; }
    public DbSet<FinishedProduct> FinishedProducts { get; set; }
    public DbSet<ProductionBatch> ProductionBatches { get; set; }
    public DbSet<ProductionStage> ProductionStages { get; set; }
    public DbSet<ProductionMaterial> ProductionMaterials { get; set; }
    public DbSet<XRayCheck> XRayChecks { get; set; }
    public DbSet<ReceivingForm> ReceivingForms { get; set; }
    public DbSet<DeliveryForm> DeliveryForms { get; set; }
    public DbSet<Shipment> Shipments { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Decimal precision (modey & Weight fix)
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,2)");
        }

        // PREVENT CASCADE DELETE LOOPS

        // Prevent deleting a ReceivingForm when a RawMaterial is deleted
        modelBuilder.Entity<ReceivingForm>()
            .HasOne(r => r.RawMaterial)
            .WithMany()
            .HasForeignKey(r => r.RawMaterialId)
            .OnDelete(DeleteBehavior.Restrict); // Important!

        //Prevent deleting a ReceivingForm when a Supplier is deleted (Optional safety)
        modelBuilder.Entity<ReceivingForm>()
            .HasOne(r => r.Supplier)
            .WithMany()
            .HasForeignKey(r => r.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<XRayCheck>()
            .HasOne(x => x.Operator)
            .WithMany()
            .HasForeignKey(x => x.OperatorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductionBatch>()
        .HasOne(p => p.Supervisor)
        .WithMany()
        .HasForeignKey(p => p.SupervisorId)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReceivingForm>()
        .HasOne(r => r.ReceivedBy)
        .WithMany()
        .HasForeignKey(r => r.ReceivedById)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeliveryForm>()
        .HasOne(d => d.ApprovedBy)
        .WithMany()
        .HasForeignKey(d => d.ApprovedById)
        .OnDelete(DeleteBehavior.Restrict);
    }
}