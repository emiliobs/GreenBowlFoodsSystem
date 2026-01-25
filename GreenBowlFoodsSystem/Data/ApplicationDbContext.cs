using GreenBowlFoodsSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace GreenBowlFoodsSystem.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
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

        // unique username constraint
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

        // Decimal precision (modey & Weight fix)
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,2)");
        }

        // PREVENT CASCADE DELETE LOOPS
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

        // Seed data (Initial test Data)

        // User
        modelBuilder.Entity<User>().HasData(

               new User { Id = 1, Username = "Admin", Password = "123", Role = "Admin" },
               new User { Id = 2, Username = "Emilio", Password = "123", Role = "Staff" }

            );

        // Supplier
        modelBuilder.Entity<Supplier>().HasData(

            new Supplier
            {
                Id = 1,
                SupplierName = "Raaz Food Ingredients",
                ContactPerson = "Alice Johnson",
                Phone = "555-1234",
                Email = "Raaz@yopmail.com"
            }

            );

        // Customer
        modelBuilder.Entity<Customer>().HasData(

             new Customer
             {
                 Id = 1,
                 CustomerName = "Costco Wholesale",
                 Email = "costco@yopmail.com",
                 Phone = "123-1234",
                 ContactName = "Emiliano Barrera",
                 BillingAddress = "Street London 55"
             }

            );

        // Product
        modelBuilder.Entity<FinishedProduct>().HasData(

              new FinishedProduct
              {
                  Id = 1,
                  ProductName = "Quinoa Salad",
                  SKU = "QB-001",
                  UnitPrice = 12.50m,
                  QuantityAvailable = 100,
              }

            );

        // Raw Material
        modelBuilder.Entity<RawMaterial>().HasData(

               new RawMaterial
               {
                   Id = 1,
                   MaterialName = "Basmati Rice",
                   LotNumber = "L-8821",
                   QuantityInStock = 500,
                   Unit = "kg",
                   ExpiryDate = new DateTime(2025, 1, 1),
                   SupplierId = 1,
                   Supplier = null
               }

            );
    }
}