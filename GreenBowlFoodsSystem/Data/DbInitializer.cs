using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; // Necessary for createScope
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GreenBowlFoodsSystem.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            // Create a Scope to get necessary services
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

                // 1. Ensure DB exists
                context.Database.EnsureCreated();

                // 2. If users exist, assume DB is already seeded
                if (context.Users.Any())
                {
                    return;
                }

                try
                {
                    // =============================================================
                    // 1. SEED ROLES AND USERS (Identity Security)
                    // =============================================================
                    string[] roles = { "Admin", "Staff" };

                    foreach (var role in roles)
                    {
                        if (!await roleManager.RoleExistsAsync(role))
                        {
                            await roleManager.CreateAsync(new IdentityRole<int>(role));
                        }
                    }

                    // --- CREATE ADMIN USER ---
                    var AdminUsers = new User
                    {
                        UserName = "admin@yopmail.com", // Login ID
                        Email = "admin@yopmail.com",
                        FirstName = "System",
                        LastName = "Administrator",
                        Role = "Admin",
                        EmailConfirmed = true,
                        PhoneNumber = "1234567890"
                    };

                    var resultAdmin = await userManager.CreateAsync(AdminUsers, "123");
                    if (resultAdmin.Succeeded)
                    {
                        await userManager.AddToRoleAsync(AdminUsers, "Admin");
                    }

                    var emilioAdmin = new User
                    {
                        UserName = "emilio@yopmail.com",
                        Email = "emilio@yopmail.com",
                        FirstName = "Emilio",
                        LastName = "Barrera",
                        Role = "Admin",
                        EmailConfirmed = true,
                        PhoneNumber = "07907951284"
                    };

                    var resulEmilioAdmin = await userManager.CreateAsync(emilioAdmin, "123");
                    if (resulEmilioAdmin.Succeeded)
                    {
                        await userManager.AddToRoleAsync(emilioAdmin, "Admin");
                    }

                    // --- CREATE STAFF USER ---
                    var staffUser = new User
                    {
                        UserName = "staff@yopmail.com",
                        Email = "staff@yopmail.com",
                        FirstName = "Juan",
                        LastName = "Perez",
                        Role = "Staff",
                        EmailConfirmed = true,
                        PhoneNumber = "0987654321"
                    };

                    var resultStaff = await userManager.CreateAsync(staffUser, "123");
                    if (resultStaff.Succeeded)
                    {
                        await userManager.AddToRoleAsync(staffUser, "Staff");
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
                        await context.SaveChangesAsync();
                    }

                    // =============================================================
                    // 3. SEED RAW MATERIALS
                    // =============================================================
                    if (!context.RawMaterials.Any())
                    {
                        var freshFarms = context.Suppliers.FirstOrDefault(s => s.SupplierName == "Fresh Farms Ltd");
                        var organicGlobal = context.Suppliers.FirstOrDefault(s => s.SupplierName == "Organic Global Imports");
                        var grainMasters = context.Suppliers.FirstOrDefault(s => s.SupplierName == "Grain Masters Inc.");
                        var spiceWorld = context.Suppliers.FirstOrDefault(s => s.SupplierName == "Spice World");

                        if (freshFarms != null && organicGlobal != null && grainMasters != null && spiceWorld != null)
                        {
                            var materials = new RawMaterial[]
                            {
                                // Fresh Farms
                                new RawMaterial { MaterialName = "Spinach (Fresh)", LotNumber = "S-2026-001", QuantityInStock = 500, Unit = "Kg", ExpiryDate = DateTime.Now.AddDays(10), SupplierId = freshFarms.Id },
                                new RawMaterial { MaterialName = "Carrots (Organic)", LotNumber = "C-2026-055", QuantityInStock = 1200, Unit = "Kg", ExpiryDate = DateTime.Now.AddDays(20), SupplierId = freshFarms.Id },
                                new RawMaterial { MaterialName = "Kale", LotNumber = "K-2026-101", QuantityInStock = 300, Unit = "Kg", ExpiryDate = DateTime.Now.AddDays(8), SupplierId = freshFarms.Id },

                                // Organic Global
                                new RawMaterial { MaterialName = "Quinoa (White)", LotNumber = "Q-9921", QuantityInStock = 2000, Unit = "Kg", ExpiryDate = DateTime.Now.AddMonths(12), SupplierId = organicGlobal.Id },
                                new RawMaterial { MaterialName = "Chickpeas (Dried)", LotNumber = "CH-5512", QuantityInStock = 1500, Unit = "Kg", ExpiryDate = DateTime.Now.AddMonths(18), SupplierId = organicGlobal.Id },
                                new RawMaterial { MaterialName = "Olive Oil (Extra Virgin)", LotNumber = "OL-221", QuantityInStock = 500, Unit = "Liters", ExpiryDate = DateTime.Now.AddMonths(24), SupplierId = organicGlobal.Id },

                                // Grain Masters
                                new RawMaterial { MaterialName = "Brown Rice", LotNumber = "BR-881", QuantityInStock = 40, Unit = "Kg", ExpiryDate = DateTime.Now.AddDays(-5), SupplierId = grainMasters.Id },
                                new RawMaterial { MaterialName = "Lentils", LotNumber = "LN-332", QuantityInStock = 800, Unit = "Kg", ExpiryDate = DateTime.Now.AddMonths(6), SupplierId = grainMasters.Id },

                                // Spice World
                                new RawMaterial { MaterialName = "Turmeric Powder", LotNumber = "SP-001", QuantityInStock = 50, Unit = "Kg", ExpiryDate = DateTime.Now.AddYears(2), SupplierId = spiceWorld.Id },
                                new RawMaterial { MaterialName = "Black Pepper", LotNumber = "SP-002", QuantityInStock = 30, Unit = "Kg", ExpiryDate = DateTime.Now.AddYears(2), SupplierId = spiceWorld.Id }
                            };
                            context.RawMaterials.AddRange(materials);
                            await context.SaveChangesAsync();
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
                        await context.SaveChangesAsync();
                    }

                    // =============================================================
                    // 5. SEED PRODUCTION BATCHES
                    // =============================================================
                    if (!context.ProductionBatches.Any())
                    {
                        var adminUser = await userManager.FindByEmailAsync("admin@yopmail.com");
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
                                    EndDate = DateTime.Now.AddDays(-5).AddHours(4),
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
                            await context.SaveChangesAsync();
                        }
                    }

                    // =============================================================
                    // 6. SEED PRODUCTION MATERIALS
                    // =============================================================
                    if (!context.ProductionMaterials.Any())
                    {
                        var batch1 = context.ProductionBatches.FirstOrDefault(b => b.BatchNumber == "BATCH-2026-001");
                        var quinoa = context.RawMaterials.FirstOrDefault(m => m.MaterialName.Contains("Quinoa"));
                        var spinach = context.RawMaterials.FirstOrDefault(m => m.MaterialName.Contains("Spinach"));
                        var oliveOil = context.RawMaterials.FirstOrDefault(m => m.MaterialName.Contains("Olive Oil"));

                        if (batch1 != null && quinoa != null && spinach != null && oliveOil != null)
                        {
                            var materialsUsed = new ProductionMaterial[]
                            {
                                new ProductionMaterial { ProductionBatchId = batch1.Id, RawMaterialId = quinoa.Id, QuantityUsed = 20.0m },
                                new ProductionMaterial { ProductionBatchId = batch1.Id, RawMaterialId = spinach.Id, QuantityUsed = 5.0m },
                                new ProductionMaterial { ProductionBatchId = batch1.Id, RawMaterialId = oliveOil.Id, QuantityUsed = 2.0m }
                            };
                            context.ProductionMaterials.AddRange(materialsUsed);
                            await context.SaveChangesAsync();
                        }
                    }

                    // =============================================================
                    // 7. SEED CUSTOMERS
                    // =============================================================
                    if (!context.Customers.Any())
                    {
                        var customers = new Customer[]
                        {
                            new Customer { CustomerName = "Costco Wholesale", ContactName = "Manager Toronto", Email = "orders@costco.ca", Phone = "416-555-0199", BillingAddress = "123 Overlea Blvd, Toronto, ON" },
                            new Customer { CustomerName = "Whole Foods Market", ContactName = "Sarah Jenkins", Email = "procurement@wholefoods.com", Phone = "647-555-0122", BillingAddress = "87 Avenue Rd, Toronto, ON" },
                            new Customer { CustomerName = "Amazon Fresh", ContactName = "Central Depot", Email = "vendor-central@amazon.ca", Phone = "905-555-0188", BillingAddress = "6363 Millcreek Dr, Mississauga, ON" }
                        };
                        context.Customers.AddRange(customers);
                        await context.SaveChangesAsync();
                    }

                    // =============================================================
                    // 8. SEED SHIPMENTS
                    // =============================================================
                    if (!context.Shipments.Any())
                    {
                        var costco = context.Customers.FirstOrDefault(c => c.CustomerName == "Costco Wholesale");
                        var wholeFoods = context.Customers.FirstOrDefault(c => c.CustomerName == "Whole Foods Market");
                        var quinoaSalad = context.FinishedProducts.FirstOrDefault(p => p.SKU == "GBF-001");

                        if (costco != null && wholeFoods != null && quinoaSalad != null)
                        {
                            var shipments = new Shipment[]
                            {
                                new Shipment
                                {
                                    Date = DateTime.Now.AddDays(-2),
                                    Carrier = "FedEx Ground",
                                    TrackingNumber = "TRK-99887766",
                                    CustomerId = costco.Id,
                                    FinishedProductId = quinoaSalad.Id,
                                    QuantityShipped = 50,
                                    TotalValue = 50 * quinoaSalad.UnitPrice,
                                    Status = "Shipped",
                                    DeliveryFormId = null
                                },
                                new Shipment
                                {
                                    Date = DateTime.Now,
                                    Carrier = "DHL Express",
                                    TrackingNumber = "DHL-11223344",
                                    CustomerId = wholeFoods.Id,
                                    FinishedProductId = quinoaSalad.Id,
                                    QuantityShipped = 20,
                                    TotalValue = 20 * quinoaSalad.UnitPrice,
                                    Status = "Pending",
                                    DeliveryFormId = null
                                }
                            };

                            context.Shipments.AddRange(shipments);

                            // CRITICAL: Deduct Stock
                            quinoaSalad.QuantityAvailable -= 70; // 50 + 20
                            context.Update(quinoaSalad);

                            await context.SaveChangesAsync();
                        }
                    }

                    // =============================================================
                    // 9. SEED X-RAY CHECKS
                    // =============================================================
                    if (!context.XRayChecks.Any())
                    {
                        var inspector = await userManager.FindByEmailAsync("admin@yopmail.com");
                        var batches = context.ProductionBatches.ToList();

                        if (inspector != null && batches.Count >= 3)
                        {
                            var xRayChecks = new XRayCheck[]
                            {
                                new XRayCheck
                                {
                                    ProductionBatchId = batches[0].Id,
                                    OperatorId = inspector.Id,
                                    CheckTime = DateTime.Now.AddDays(-1),
                                    Result = "Pass",
                                    Comments = "Routine scan completed. No contaminants found. Approved."
                                },
                                new XRayCheck
                                {
                                    ProductionBatchId = batches[1].Id,
                                    OperatorId = inspector.Id,
                                    CheckTime = DateTime.Now,
                                    Result = "Fail",
                                    Comments = "CRITICAL: Small metal fragment detected. Batch quarantined."
                                },
                                new XRayCheck
                                {
                                    ProductionBatchId = batches[3].Id,
                                    OperatorId = inspector.Id,
                                    CheckTime = DateTime.Now,
                                    Result = "Pass",
                                    Comments = "Routine scan completed. No contaminants found. Approved."
                                }
                            };
                            context.XRayChecks.AddRange(xRayChecks);
                            await context.SaveChangesAsync();
                        }
                    }

                    // =============================================================
                    // 10. SEED RECEIVING FORMS (UPDATED WITH MATERIAL & QTY)
                    // =============================================================
                    if (!context.ReceivingForms.Any())
                    {
                        // 1. Get User & Suppliers
                        var adminUser = await userManager.FindByEmailAsync("admin@yopmail.com");
                        var freshFarms = context.Suppliers.FirstOrDefault(s => s.SupplierName == "Fresh Farms Ltd");
                        var organicGlobal = context.Suppliers.FirstOrDefault(s => s.SupplierName == "Organic Global Imports");

                        // 2. Get Raw Materials (We need their IDs now!)
                        var spinach = context.RawMaterials.FirstOrDefault(m => m.MaterialName.Contains("Spinach"));
                        var quinoa = context.RawMaterials.FirstOrDefault(m => m.MaterialName.Contains("Quinoa"));
                        var carrots = context.RawMaterials.FirstOrDefault(m => m.MaterialName.Contains("Carrots"));

                        // Check if everything exists before seeding
                        if (adminUser != null && freshFarms != null && organicGlobal != null &&
                            spinach != null && quinoa != null && carrots != null)
                        {
                            var receipts = new ReceivingForm[]
                            {
            // Receipt 1: Accepted Spinach Load
            new ReceivingForm
            {
                Date = DateTime.Now.AddDays(-10),
                SupplierId = freshFarms.Id,
                TrailerNumber = "TR-8854",
                IsAccepted = true,
                InspectionNotes = "Fresh shipment of Spinach. Temp OK. Seals intact.",
                TotalAmount = 450.00m,
                ReceivedById = adminUser.Id,
                // --- NEW REQUIRED FIELDS ---
                RawMaterialId = spinach.Id,
                QuantityReceived = 100.00m
            },
            // Receipt 2: Rejected Quinoa Load (Damaged)
            new ReceivingForm
            {
                Date = DateTime.Now.AddDays(-5),
                SupplierId = organicGlobal.Id,
                TrailerNumber = "OG-9921",
                IsAccepted = false,
                InspectionNotes = "REJECTED: Pallets arrived water damaged. Mold visible.",
                TotalAmount = 1200.00m,
                ReceivedById = adminUser.Id,
                // --- NEW REQUIRED FIELDS ---
                RawMaterialId = quinoa.Id,
                QuantityReceived = 500.00m
            },
            // Receipt 3: Accepted Carrots
            new ReceivingForm
            {
                Date = DateTime.Now.AddDays(-2),
                SupplierId = freshFarms.Id,
                TrailerNumber = "TR-9901",
                IsAccepted = true,
                InspectionNotes = "Standard weekly delivery. All good.",
                TotalAmount = 800.50m,
                ReceivedById = adminUser.Id,
                // --- NEW REQUIRED FIELDS ---
                RawMaterialId = carrots.Id,
                QuantityReceived = 200.00m
            }
                            };
                            context.ReceivingForms.AddRange(receipts);
                            await context.SaveChangesAsync();
                        }
                    }

                    // =============================================================
                    // 11. SEED PACKAGING MATERIALS (NEW!)
                    // =============================================================
                    if (!context.PackagingMaterials.Any())
                    {
                        // Get Suppliers
                        var greenPack = context.Suppliers.FirstOrDefault(s => s.SupplierName == "Green Packaging Solutions");
                        var freshFarms = context.Suppliers.FirstOrDefault(s => s.SupplierName == "Fresh Farms Ltd");

                        if (greenPack != null && freshFarms != null)
                        {
                            var packagingItems = new PackagingMaterial[]
                            {
                                // From Green Packaging Solutions
                                new PackagingMaterial
                                {
                                    MaterialName = "Cardboard Box 10x10x10",
                                    QuantityInStock = 5000,
                                    SupplierId = greenPack.Id
                                },
                                new PackagingMaterial
                                {
                                    MaterialName = "Biodegradable Salad Bowl (Large)",
                                    QuantityInStock = 2500,
                                    SupplierId = greenPack.Id
                                },
                                new PackagingMaterial
                                {
                                    MaterialName = "Plastic Seal Wrap (Roll)",
                                    QuantityInStock = 50,
                                    SupplierId = greenPack.Id
                                },
                                new PackagingMaterial
                                {
                                    MaterialName = "Label Sticker - 'Organic'",
                                    QuantityInStock = 10000,
                                    SupplierId = greenPack.Id
                                },

                                // From Fresh Farms (Maybe crates?)
                                new PackagingMaterial
                                {
                                    MaterialName = "Reusable Plastic Crate (Blue)",
                                    QuantityInStock = 200,
                                    SupplierId = freshFarms.Id
                                }
                            };

                            context.PackagingMaterials.AddRange(packagingItems);
                            await context.SaveChangesAsync();
                        }
                    }

                    // =============================================================
                    // 12. SEED PRODUCTION STAGES (PROCESS LOG)
                    // =============================================================
                    if (!context.ProductionStages.Any())
                    {
                        var batch1 = context.ProductionBatches.FirstOrDefault(b => b.BatchNumber == "BATCH-2026-001");

                        if (batch1 != null)
                        {
                            var stages = new ProductionStage[]
                            {
                                // Step 1: Weighing Ingredients
                                new ProductionStage
                                {
                                    ProductionBatchId = batch1.Id,
                                    StageName = StageType.Weighing,
                                    StartTime = batch1.ProductionDate,
                                    EndTime = batch1.ProductionDate.AddMinutes(30),
                                    TemperatureCelsius = 18.5m, // Room temp
                                    Notes = "All ingredients weighed and verified against BOM."
                                },
                                // Step 2: Mixing
                                new ProductionStage
                                {
                                    ProductionBatchId = batch1.Id,
                                    StageName = StageType.Mixing,
                                    StartTime = batch1.ProductionDate.AddMinutes(35),
                                    EndTime = batch1.ProductionDate.AddMinutes(55),
                                    TemperatureCelsius = 20.0m,
                                    Notes = "Standard mixing speed. Consistency looks good."
                                },
                                // Step 3: Cooking (Critical Control Point)
                                new ProductionStage
                                {
                                    ProductionBatchId = batch1.Id,
                                    StageName = StageType.Cooking,
                                    StartTime = batch1.ProductionDate.AddMinutes(60),
                                    EndTime = batch1.ProductionDate.AddMinutes(120),
                                    TemperatureCelsius = 85.5m, // Hot!
                                    Notes = "Reached target temperature of 85C for 30 mins. Safe."
                                },
                                // Step 4: Quality Check
                                new ProductionStage
                                {
                                    ProductionBatchId = batch1.Id,
                                    StageName = StageType.QualityCheck,
                                    StartTime = batch1.ProductionDate.AddMinutes(125),
                                    EndTime = batch1.ProductionDate.AddMinutes(140),
                                    TemperatureCelsius = 45.0m, // Cooling down
                                    Notes = "Taste test passed. Color is vibrant. Approved for packaging."
                                }
                            };
                            context.ProductionStages.AddRange(stages);
                            await context.SaveChangesAsync();
                        }
                    }

                    // =============================================================
                    // 13. SEED LOGISTICS (DELIVERY & SHIPMENTS)
                    // =============================================================
                    if (!context.DeliveryForms.Any())
                    {
                        var adminUser = await userManager.FindByEmailAsync("admin@yopmail.com");
                        // Get the first Customer (e.g., Costco)
                        var customer = context.Customers.FirstOrDefault();
                        // Get the Finished Product (e.g., Salad)
                        var product = context.FinishedProducts.FirstOrDefault();

                        if (adminUser != null && customer != null && product != null)
                        {
                            // 1. Create the Vehicle Check Record (DeliveryForm)
                            var deliveryForm = new DeliveryForm
                            {
                                CheckDate = DateTime.Now.AddDays(-1), // Yesterday
                                TrailerNumber = "TR-8855-X",
                                DriverName = "Michael Schum",
                                IsTempOk = true,
                                IsClean = true,
                                ApprovedById = adminUser.Id
                            };

                            context.DeliveryForms.Add(deliveryForm);
                            await context.SaveChangesAsync(); // Save to generate the ID

                            // 2. Create the Shipment that was loaded onto that truck
                            var shipment = new Shipment
                            {
                                Date = DateTime.Now.AddDays(-1),
                                Carrier = "DHL Cold Chain",
                                TrackingNumber = "DHL-99887766",
                                CustomerId = customer.Id,
                                FinishedProductId = product.Id,
                                QuantityShipped = 50,
                                TotalValue = 50 * product.UnitPrice,
                                Status = "Shipped", // Status: Departed
                                DeliveryFormId = deliveryForm.Id // <--- CRITICAL LINK: Order linked to Truck
                            };

                            // Deduct form Inventory (Simulation)
                            product.QuantityAvailable -= 50;

                            context.Shipments.Add(shipment);
                            context.Update(product);
                            await context.SaveChangesAsync();
                        }
                    }

                    // =============================================================
                    // 14. SEED FINANCE (4 INVOICES & DETAILS)
                    // =============================================================
                    if (!context.Invoices.Any())
                    {
                        // Obtener referencias (Clientes y Productos)
                        var customer1 = context.Customers.OrderBy(c => c.Id).FirstOrDefault(); // Ej: Costco
                        var customer2 = context.Customers.OrderBy(c => c.Id).Skip(1).FirstOrDefault() ?? customer1; // Ej: Tesco (o Costco si no hay más)

                        var product1 = context.FinishedProducts.OrderBy(p => p.Id).FirstOrDefault(); // Ej: Quinopea
                        var product2 = context.FinishedProducts.OrderBy(p => p.Id).Skip(1).FirstOrDefault() ?? product1; // Ej: Otro producto

                        if (customer1 != null && product1 != null)
                        {
                            // --- ESCENARIO 1: FACTURA VENCIDA (OVERDUE) ---
                            // Fecha: hace 45 días (venció hace 15)
                            var inv1 = new Invoice
                            {
                                InvoiceNumber = "INV-2025-901",
                                CustomerId = customer1.Id,
                                Date = DateTime.Now.AddDays(-45),
                                Status = "Overdue",
                                TotalAmount = 0
                            };
                            context.Invoices.Add(inv1);
                            await context.SaveChangesAsync();

                            var item1 = new InvoiceItem
                            {
                                InvoiceId = inv1.Id,
                                FinishedProductId = product1.Id,
                                Quantity = 500,
                                UnitPrice = 10.00m
                            };
                            context.InvoiceItems.Add(item1);
                            inv1.TotalAmount = item1.Quantity * item1.UnitPrice; // Total: 5000
                            context.Update(inv1);


                            // --- ESCENARIO 2: FACTURA PAGADA (PAID) ---
                            // Fecha: hace 10 días
                            var inv2 = new Invoice
                            {
                                InvoiceNumber = "INV-2026-005",
                                CustomerId = customer1.Id,
                                Date = DateTime.Now.AddDays(-10),
                                Status = "Paid",
                                TotalAmount = 0
                            };
                            context.Invoices.Add(inv2);
                            await context.SaveChangesAsync();

                            var item2 = new InvoiceItem
                            {
                                InvoiceId = inv2.Id,
                                FinishedProductId = product2.Id,
                                Quantity = 150,
                                UnitPrice = 15.50m
                            };
                            context.InvoiceItems.Add(item2);
                            inv2.TotalAmount = item2.Quantity * item2.UnitPrice; // Total: 2325
                            context.Update(inv2);


                            // --- ESCENARIO 3: FACTURA RECIENTE (UNPAID) ---
                            // Fecha: Ayer
                            var inv3 = new Invoice
                            {
                                InvoiceNumber = "INV-2026-012",
                                CustomerId = customer2.Id,
                                Date = DateTime.Now.AddDays(-1),
                                Status = "Unpaid",
                                TotalAmount = 0
                            };
                            context.Invoices.Add(inv3);
                            await context.SaveChangesAsync();

                            var item3 = new InvoiceItem
                            {
                                InvoiceId = inv3.Id,
                                FinishedProductId = product1.Id,
                                Quantity = 1000,
                                UnitPrice = 12.00m
                            };
                            context.InvoiceItems.Add(item3);
                            inv3.TotalAmount = item3.Quantity * item3.UnitPrice; // Total: 12000
                            context.Update(inv3);


                            // --- ESCENARIO 4: FACTURA COMPLEJA (MÚLTIPLES ÍTEMS) ---
                            // Demuestra la relación One-to-Many real
                            var inv4 = new Invoice
                            {
                                InvoiceNumber = "INV-2026-015",
                                CustomerId = customer2.Id,
                                Date = DateTime.Now,
                                Status = "Unpaid",
                                TotalAmount = 0
                            };
                            context.Invoices.Add(inv4);
                            await context.SaveChangesAsync();

                            // Ítem A: Ensaladas
                            var item4a = new InvoiceItem
                            {
                                InvoiceId = inv4.Id,
                                FinishedProductId = product1.Id,
                                Quantity = 200,
                                UnitPrice = 12.00m
                            };
                            // Ítem B: Sopas (u otro producto)
                            var item4b = new InvoiceItem
                            {
                                InvoiceId = inv4.Id,
                                FinishedProductId = product2.Id,
                                Quantity = 300,
                                UnitPrice = 18.00m
                            };

                            context.InvoiceItems.AddRange(item4a, item4b);

                            // Sumar ambos ítems para el total
                            inv4.TotalAmount = (item4a.Quantity * item4a.UnitPrice) + (item4b.Quantity * item4b.UnitPrice);
                            context.Update(inv4);

                            // GUARDAR TODO FINALMENTE
                            await context.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
                }
            }
        }
    }
}