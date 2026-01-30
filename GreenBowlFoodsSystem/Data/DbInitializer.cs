using GreenBowlFoodsSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace GreenBowlFoodsSystem.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        // Ensure the database is created
        context.Database.EnsureCreated();

        // =============================================================
        // 1. SEED USERS (Solo si no existen)
        // =============================================================
        if (!context.Users.Any())
        {
            var users = new User[]
            {
                new User { Username = "Admin", Password = "123", Role = "Admin" },
                new User { Username = "Staff", Password = "123", Role = "Staff" },
                new User { Username = "Manager", Password = "123", Role = "Admin" },
                new User { Username = "Worker", Password = "123", Role = "Staff" }
            };
            context.Users.AddRange(users);
            context.SaveChanges();
        }

        // =============================================================
        // 2. SEED SUPPLIERS
        // =============================================================
        if (!context.Suppliers.Any())
        {
            var suppliers = new Supplier[]
            {
                new Supplier { SupplierName = "Fresh Farms Ltd", ContactPerson = "John Smith", Email = "orders@freshfarms.com", Phone = "416-555-0101" },
                new Supplier { SupplierName = "Organic Global Imports", ContactPerson = "Maria Garcia", Email = "maria@organicglobal.com", Phone = "416-555-0102" },
                new Supplier { SupplierName = "Green Packaging Solutions", ContactPerson = "David Lee", Email = "sales@greenpack.com", Phone = "416-555-0103" },
                new Supplier { SupplierName = "Grain Masters Inc.", ContactPerson = "Sarah Connor", Email = "s.connor@grainmasters.ca", Phone = "416-555-0104" },
                new Supplier { SupplierName = "Spice World", ContactPerson = "Raj Patel", Email = "raj@spiceworld.com", Phone = "416-555-0105" }
            };
            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();
        }

        // =============================================================
        // 3. SEED RAW MATERIALS
        // =============================================================
        if (!context.RawMaterials.Any())
        {
            // Buscamos los proveedores en la BD para obtener sus IDs reales
            var freshFarms = context.Suppliers.FirstOrDefault(s => s.SupplierName == "Fresh Farms Ltd");
            var organicGlobal = context.Suppliers.FirstOrDefault(s => s.SupplierName == "Organic Global Imports");
            var greenPack = context.Suppliers.FirstOrDefault(s => s.SupplierName == "Green Packaging Solutions");
            var grainMasters = context.Suppliers.FirstOrDefault(s => s.SupplierName == "Grain Masters Inc.");
            var spiceWorld = context.Suppliers.FirstOrDefault(s => s.SupplierName == "Spice World");

            // Solo insertamos si encontramos los proveedores
            if (freshFarms != null && organicGlobal != null && grainMasters != null && spiceWorld != null)
            {
                var materials = new RawMaterial[]
                {
                    // Supplier: Fresh Farms
                    new RawMaterial { MaterialName = "Spinach (Fresh)", LotNumber = "S-2026-001", QuantityInStock = 500, Unit = "Kg", ExpiryDate = DateTime.Now.AddDays(10), SupplierId = freshFarms.Id },
                    new RawMaterial { MaterialName = "Carrots (Organic)", LotNumber = "C-2026-055", QuantityInStock = 1200, Unit = "Kg", ExpiryDate = DateTime.Now.AddDays(20), SupplierId = freshFarms.Id },
                    new RawMaterial { MaterialName = "Kale", LotNumber = "K-2026-101", QuantityInStock = 300, Unit = "Kg", ExpiryDate = DateTime.Now.AddDays(8), SupplierId = freshFarms.Id },

                    // Supplier: Organic Global
                    new RawMaterial { MaterialName = "Quinoa (White)", LotNumber = "Q-9921", QuantityInStock = 2000, Unit = "Kg", ExpiryDate = DateTime.Now.AddMonths(12), SupplierId = organicGlobal.Id },
                    new RawMaterial { MaterialName = "Chickpeas (Dried)", LotNumber = "CH-5512", QuantityInStock = 1500, Unit = "Kg", ExpiryDate = DateTime.Now.AddMonths(18), SupplierId = organicGlobal.Id },
                    new RawMaterial { MaterialName = "Olive Oil (Extra Virgin)", LotNumber = "OL-221", QuantityInStock = 500, Unit = "Liters", ExpiryDate = DateTime.Now.AddMonths(24), SupplierId = organicGlobal.Id },

                    // Supplier: Grain Masters
                    new RawMaterial { MaterialName = "Brown Rice", LotNumber = "BR-881", QuantityInStock = 40, Unit = "Kg", ExpiryDate = DateTime.Now.AddDays(-5), SupplierId = grainMasters.Id },
                    new RawMaterial { MaterialName = "Lentils", LotNumber = "LN-332", QuantityInStock = 800, Unit = "Kg", ExpiryDate = DateTime.Now.AddMonths(6), SupplierId = grainMasters.Id },

                    // Supplier: Spice World
                    new RawMaterial { MaterialName = "Turmeric Powder", LotNumber = "SP-001", QuantityInStock = 50, Unit = "Kg", ExpiryDate = DateTime.Now.AddYears(2), SupplierId = spiceWorld.Id },
                    new RawMaterial { MaterialName = "Black Pepper", LotNumber = "SP-002", QuantityInStock = 30, Unit = "Kg", ExpiryDate = DateTime.Now.AddYears(2), SupplierId = spiceWorld.Id }
                };
                context.RawMaterials.AddRange(materials);
                context.SaveChanges();
            }
        }

        // =============================================================
        // 4. SEED FINISHED PRODUCTS
        // =============================================================
        if (!context.FinishedProducts.Any())
        {
            var products = new FinishedProduct[]
            {
                new FinishedProduct { ProductName = "Zesty Quinoa Salad", SKU = "GBF-001", UnitPrice = 12.50m, QuantityAvailable = 150 },
                new FinishedProduct { ProductName = "Green Power Bowl", SKU = "GBF-002", UnitPrice = 14.00m, QuantityAvailable = 80 },
                new FinishedProduct { ProductName = "Spicy Lentil Wrap", SKU = "GBF-003", UnitPrice = 9.50m, QuantityAvailable = 5 },
                new FinishedProduct { ProductName = "Mediterranean Chickpea Salad", SKU = "GBF-004", UnitPrice = 11.00m, QuantityAvailable = 200 },
                new FinishedProduct { ProductName = "Vegan Buddha Bowl", SKU = "GBF-005", UnitPrice = 15.50m, QuantityAvailable = 45 },
                new FinishedProduct { ProductName = "Carrot & Ginger Soup", SKU = "GBF-006", UnitPrice = 8.00m, QuantityAvailable = 300 },
                new FinishedProduct { ProductName = "Spinach & Kale Smoothie", SKU = "GBF-007", UnitPrice = 7.50m, QuantityAvailable = 0 },
                new FinishedProduct { ProductName = "Protein Power Box", SKU = "GBF-008", UnitPrice = 13.50m, QuantityAvailable = 120 },
                new FinishedProduct { ProductName = "Roasted Veggie Pasta", SKU = "GBF-009", UnitPrice = 12.00m, QuantityAvailable = 60 },
                new FinishedProduct { ProductName = "Tofu Stir Fry", SKU = "GBF-010", UnitPrice = 14.50m, QuantityAvailable = 90 },
                new FinishedProduct { ProductName = "Mango Tango Smoothie", SKU = "GBF-011", UnitPrice = 8.50m, QuantityAvailable = 45 },
                new FinishedProduct { ProductName = "Avocado Toast Kit", SKU = "GBF-012", UnitPrice = 10.00m, QuantityAvailable = 15 },
                new FinishedProduct { ProductName = "Berry Blast Bowl", SKU = "GBF-013", UnitPrice = 11.50m, QuantityAvailable = 200 },
                new FinishedProduct { ProductName = "Teriyaki Chicken (Vegan)", SKU = "GBF-014", UnitPrice = 13.00m, QuantityAvailable = 60 },
                new FinishedProduct { ProductName = "Cauliflower Wings", SKU = "GBF-015", UnitPrice = 9.00m, QuantityAvailable = 0 },
                new FinishedProduct { ProductName = "Falafel Hummus Box", SKU = "GBF-016", UnitPrice = 10.50m, QuantityAvailable = 120 },
                new FinishedProduct { ProductName = "Keto Cobb Salad", SKU = "GBF-017", UnitPrice = 16.00m, QuantityAvailable = 30 },
                new FinishedProduct { ProductName = "Sweet Potato Mash", SKU = "GBF-018", UnitPrice = 7.00m, QuantityAvailable = 500 },
                new FinishedProduct { ProductName = "Asian Slaw Side", SKU = "GBF-019", UnitPrice = 5.50m, QuantityAvailable = 5 },
                new FinishedProduct { ProductName = "Ginger Ale (Craft)", SKU = "GBF-020", UnitPrice = 3.50m, QuantityAvailable = 1000 }
            };
            context.FinishedProducts.AddRange(products);
            context.SaveChanges();
        }

        // =============================================================
        // 5. SEED PRODUCTION BATCHES (¡Lo que faltaba!)
        // =============================================================
        if (!context.ProductionBatches.Any())
        {
            // Obtenemos los IDs frescos de la base de datos
            var adminUser = context.Users.FirstOrDefault(u => u.Username == "Admin");
            var quinoaSalad = context.FinishedProducts.FirstOrDefault(p => p.SKU == "GBF-001");
            var greenBowl = context.FinishedProducts.FirstOrDefault(p => p.SKU == "GBF-002");

            if (adminUser != null && quinoaSalad != null && greenBowl != null)
            {
                var batches = new ProductionBatch[]
                {
                    new ProductionBatch
                    {
                        BatchNumber = "BATCH-2026-001",
                        ProductionDate = DateTime.Now.AddDays(-5),
                        FinishedProductId = quinoaSalad.Id,
                        SupervisorId = adminUser.Id,
                        TargetQuantity = 100,
                        QuantityProduced = 98,
                        DowntimeMinutes = 0,
                        Status = "Completed"
                    },
                    new ProductionBatch
                    {
                        BatchNumber = "BATCH-2026-002",
                        ProductionDate = DateTime.Now.AddDays(-3),
                        FinishedProductId = greenBowl.Id,
                        SupervisorId = adminUser.Id,
                        TargetQuantity = 50,
                        QuantityProduced = 40,
                        DowntimeMinutes = 45,
                        Status = "QA Hold"
                    },
                    new ProductionBatch
                    {
                        BatchNumber = "BATCH-2026-003",
                        ProductionDate = DateTime.Now,
                        FinishedProductId = quinoaSalad.Id,
                        SupervisorId = adminUser.Id,
                        TargetQuantity = 200,
                        QuantityProduced = 0,
                        DowntimeMinutes = 0,
                        Status = "Planned"
                    },
                    new ProductionBatch
                    {
                        BatchNumber = "BATCH-2026-004",
                        ProductionDate = DateTime.Now,
                        FinishedProductId = greenBowl.Id,
                        SupervisorId = adminUser.Id,
                        TargetQuantity = 150,
                        QuantityProduced = 75,
                        DowntimeMinutes = 5,
                        Status = "In Progress"
                    }
                };

                context.ProductionBatches.AddRange(batches);
                context.SaveChanges();
            }
        }

        // =============================================================
        // 6. SEED PRODUCTION MATERIALS (Ingredients used in a Batch)
        // =============================================================

        // Check if Materials are already assigned to batches
        if (!context.ProductionMaterials.Any())
        {
            // Get the Batch (The Parent)
            // We look for the completed batch we created earlier
            var batch1 = context.ProductionBatches.FirstOrDefault(b => b.BatchNumber == "BATCH-2026-001");

            // Get the Ingredients (Raw Materials)
            var quinoa = context.RawMaterials.FirstOrDefault(m => m.MaterialName.Contains("Quinoa"));
            var spinach = context.RawMaterials.FirstOrDefault(m => m.MaterialName.Contains("Spinach"));
            var oliveOil = context.RawMaterials.FirstOrDefault(m => m.MaterialName.Contains("Olive Oil"));

            //  Create the Usage Records
            if (batch1 != null && quinoa != null && spinach != null && oliveOil != null)
            {
                var materialsUsed = new ProductionMaterial[]
                {
                        // Log: Used 20kg of Quinoa for Batch 001
                        new ProductionMaterial
                        {
                            ProductionBatchId = batch1.Id,
                            RawMaterialId = quinoa.Id,
                            QuantityUsed = 20.0m
                        },

                        // Log: Used 5kg of Spinach for Batch 001
                        new ProductionMaterial
                        {
                            ProductionBatchId = batch1.Id,
                            RawMaterialId = spinach.Id,
                            QuantityUsed = 5.0m
                        },

                        // Log: Used 2 Liters of Oil for Batch 001
                        new ProductionMaterial
                        {
                            ProductionBatchId = batch1.Id,
                            RawMaterialId = oliveOil.Id,
                            QuantityUsed = 2.0m
                        }
                };

                context.ProductionMaterials.AddRange(materialsUsed);
                context.SaveChanges();
            }
        }
    }
}