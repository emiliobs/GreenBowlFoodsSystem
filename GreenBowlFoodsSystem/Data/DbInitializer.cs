using GreenBowlFoodsSystem.Models;

namespace GreenBowlFoodsSystem.Data;

public static class DbInitializer
{
    public static void Initializer(ApplicationDbContext context)
    {
        // Ensure the database is created
        context.Database.EnsureCreated();

        // Check if data exists, if there already prodcuts, i assume the DB is seeed ans stop
        if (context.FinishedProducts.Any())
        {
            return; // DB has been seeded
        }

        // Seed Users

        var users = new User[]
             {
                new User { Username = "Admin", Password = "123", Role = "Admin" },
                new User { Username = "Staff", Password = "123", Role = "Staff" },
                new User { Username = "Manager", Password = "123", Role = "Admin" },
                new User { Username = "Worker", Password = "123", Role = "Staff" }
             };
        context.Users.AddRange(users);
        context.SaveChanges();

        // Seed suppliers (i need thse IDs fpr row Materials)
        var suppliers = new Supplier[]
            {
                new Supplier { SupplierName = "Fresh Farms Ltd", ContactPerson = "John Smith", Email = "orders@freshfarms.com", Phone = "416-555-0101" },
                new Supplier { SupplierName = "Organic Global Imports", ContactPerson = "Maria Garcia", Email = "maria@organicglobal.com", Phone = "416-555-0102" },
                new Supplier { SupplierName = "Green Packaging Solutions", ContactPerson = "David Lee", Email = "sales@greenpack.com", Phone = "416-555-0103" },
                new Supplier { SupplierName = "Grain Masters Inc.", ContactPerson = "Sarah Connor", Email = "s.connor@grainmasters.ca", Phone = "416-555-0104" },
                new Supplier { SupplierName = "Spice World", ContactPerson = "Raj Patel", Email = "raj@spiceworld.com", Phone = "416-555-0105" }
            };
        context.Suppliers.AddRange(suppliers);
        context.SaveChanges(); // Save to generate IDs

        // Seed Raw Materials (Ingredients)
        var materials = new RawMaterial[]
            {
                // Supplier 1: Fresh Farms
                new RawMaterial { MaterialName = "Spinach (Fresh)", LotNumber = "S-2026-001", QuantityInStock = 500, Unit = "Kg", ExpiryDate = DateTime.Now.AddDays(10), SupplierId = suppliers[0].Id },
                new RawMaterial { MaterialName = "Carrots (Organic)", LotNumber = "C-2026-055", QuantityInStock = 1200, Unit = "Kg", ExpiryDate = DateTime.Now.AddDays(20), SupplierId = suppliers[0].Id },
                new RawMaterial { MaterialName = "Kale", LotNumber = "K-2026-101", QuantityInStock = 300, Unit = "Kg", ExpiryDate = DateTime.Now.AddDays(8), SupplierId = suppliers[0].Id },

                // Supplier 2: Organic Global
                new RawMaterial { MaterialName = "Quinoa (White)", LotNumber = "Q-9921", QuantityInStock = 2000, Unit = "Kg", ExpiryDate = DateTime.Now.AddMonths(12), SupplierId = suppliers[1].Id },
                new RawMaterial { MaterialName = "Chickpeas (Dried)", LotNumber = "CH-5512", QuantityInStock = 1500, Unit = "Kg", ExpiryDate = DateTime.Now.AddMonths(18), SupplierId = suppliers[1].Id },
                new RawMaterial { MaterialName = "Olive Oil (Extra Virgin)", LotNumber = "OL-221", QuantityInStock = 500, Unit = "Liters", ExpiryDate = DateTime.Now.AddMonths(24), SupplierId = suppliers[1].Id },

                // Supplier 3: Packaging (Optional if you have Packaging Table later, usually mixed here for now)

                // Supplier 4: Grain Masters
                new RawMaterial { MaterialName = "Brown Rice", LotNumber = "BR-881", QuantityInStock = 40, Unit = "Kg", ExpiryDate = DateTime.Now.AddDays(-5), SupplierId = suppliers[3].Id }, // EXPIRED EXAMPLE
                new RawMaterial { MaterialName = "Lentils", LotNumber = "LN-332", QuantityInStock = 800, Unit = "Kg", ExpiryDate = DateTime.Now.AddMonths(6), SupplierId = suppliers[3].Id },

                // Supplier 5: Spice World
                new RawMaterial { MaterialName = "Turmeric Powder", LotNumber = "SP-001", QuantityInStock = 50, Unit = "Kg", ExpiryDate = DateTime.Now.AddYears(2), SupplierId = suppliers[4].Id },
                new RawMaterial { MaterialName = "Black Pepper", LotNumber = "SP-002", QuantityInStock = 30, Unit = "Kg", ExpiryDate = DateTime.Now.AddYears(2), SupplierId = suppliers[4].Id }
            };
        context.RawMaterials.AddRange(materials);
        context.SaveChanges();

        // Seed Finished Products
        var products = new FinishedProduct[]
            {
                new FinishedProduct { ProductName = "Zesty Quinoa Salad", SKU = "GBF-001", UnitPrice = 12.50m, QuantityAvailable = 150 },
                new FinishedProduct { ProductName = "Green Power Bowl", SKU = "GBF-002", UnitPrice = 14.00m, QuantityAvailable = 80 },
                new FinishedProduct { ProductName = "Spicy Lentil Wrap", SKU = "GBF-003", UnitPrice = 9.50m, QuantityAvailable = 5 }, // LOW STOCK
                new FinishedProduct { ProductName = "Mediterranean Chickpea Salad", SKU = "GBF-004", UnitPrice = 11.00m, QuantityAvailable = 200 },
                new FinishedProduct { ProductName = "Vegan Buddha Bowl", SKU = "GBF-005", UnitPrice = 15.50m, QuantityAvailable = 45 },
                new FinishedProduct { ProductName = "Carrot & Ginger Soup", SKU = "GBF-006", UnitPrice = 8.00m, QuantityAvailable = 300 },
                new FinishedProduct { ProductName = "Spinach & Kale Smoothie", SKU = "GBF-007", UnitPrice = 7.50m, QuantityAvailable = 0 }, // OUT OF STOCK
                new FinishedProduct { ProductName = "Protein Power Box", SKU = "GBF-008", UnitPrice = 13.50m, QuantityAvailable = 120 },
                new FinishedProduct { ProductName = "Roasted Veggie Pasta", SKU = "GBF-009", UnitPrice = 12.00m, QuantityAvailable = 60 },
                new FinishedProduct { ProductName = "Tofu Stir Fry", SKU = "GBF-010", UnitPrice = 14.50m, QuantityAvailable = 90 },
                new FinishedProduct { ProductName = "Mango Tango Smoothie", SKU = "GBF-011", UnitPrice = 8.50m, QuantityAvailable = 45 },
                new FinishedProduct { ProductName = "Avocado Toast Kit", SKU = "GBF-012", UnitPrice = 10.00m, QuantityAvailable = 15 }, // LOW STOCK
                new FinishedProduct { ProductName = "Berry Blast Bowl", SKU = "GBF-013", UnitPrice = 11.50m, QuantityAvailable = 200 },
                new FinishedProduct { ProductName = "Teriyaki Chicken (Vegan)", SKU = "GBF-014", UnitPrice = 13.00m, QuantityAvailable = 60 },
                new FinishedProduct { ProductName = "Cauliflower Wings", SKU = "GBF-015", UnitPrice = 9.00m, QuantityAvailable = 0 }, // OUT OF STOCK
                new FinishedProduct { ProductName = "Falafel Hummus Box", SKU = "GBF-016", UnitPrice = 10.50m, QuantityAvailable = 120 },
                new FinishedProduct { ProductName = "Keto Cobb Salad", SKU = "GBF-017", UnitPrice = 16.00m, QuantityAvailable = 30 },
                new FinishedProduct { ProductName = "Sweet Potato Mash", SKU = "GBF-018", UnitPrice = 7.00m, QuantityAvailable = 500 },
                new FinishedProduct { ProductName = "Asian Slaw Side", SKU = "GBF-019", UnitPrice = 5.50m, QuantityAvailable = 5 }, // LOW STOCK
                new FinishedProduct { ProductName = "Ginger Ale (Craft)", SKU = "GBF-020", UnitPrice = 3.50m, QuantityAvailable = 1000 }
            };
        context.FinishedProducts.AddRange(products);
        context.SaveChanges();
    }
}