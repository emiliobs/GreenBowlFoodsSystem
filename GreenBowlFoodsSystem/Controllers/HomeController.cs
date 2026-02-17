using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using GreenBowlFoodsSystem.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;// Ensure this matches the previous namespace

namespace GreenBowlFoodsSystem.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context; // Database context for analytical queries
    private readonly ILogger<HomeController> _logger;

    // Constructor: Injecting the ApplicationDbContext and Logger service
    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        this._context = context;
        this._logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        // Authentication Check: Displays an empty dashboard or landing state if not logged in
        if (!User.Identity!.IsAuthenticated)
        {
            return View(new DashboardViewModel());
        }

        // IF LOGGED IN: Aggregate Business Intelligence (BI) Data
        try
        {
            // Financial Performance - Total Revenue aggregated from the Invoices module
            var revenue = await _context.Invoices.SumAsync(i => i.TotalAmount);

            // Asset Management - Total Monetary Value of finished goods currently in stock
            var totalValue = await _context.FinishedProducts
                .SumAsync(p => p.UnitPrice * p.QuantityAvailable);

            // Logistics Pipeline - Count of active shipments that have not reached 'Delivered' status
            var activeShipments = await _context.Shipments
                .CountAsync(s => s.Status != "Delivered");

            //  Quality Compliance - Safety incidents (Failed X-Ray checks) in the last 24 hours
            var yesterday = DateTime.Now.AddHours(-24);
            var failedChecks = await _context.XRayChecks
                .CountAsync(x => x.Result == "Fail" && x.CheckTime >= yesterday);

            // Supply Chain Risk - Count of raw materials nearing their expiration date (7-day window)
            var warningDate = DateTime.Now.AddDays(7);
            var expiringCount = await _context.RawMaterials
                .CountAsync(m => m.ExpiryDate <= warningDate);

            // ViewModel Construction: Mapping raw data to the Dashboard display model
            var dashboardViewModel = new DashboardViewModel
            {
                TotalInventoryValue = totalValue,
                TotalRevenue = revenue,
                ActiveShipmentsCount = activeShipments,
                QualityIssuesToday = failedChecks,
                ExpiringSoonCount = expiringCount,

                // Data Feed 1: Recent Manufacturing Activity (Last 5 Production Batches)
                RecentBatches = await _context.ProductionBatches
                    .Include(pb => pb.FinishedProduct)
                    .OrderByDescending(pb => pb.ProductionDate)
                    .Take(5)
                    .ToListAsync(),

                // Data Feed 2: Recent Logistics Activity (Last 5 Shipments dispatched)
                RecentShipments = await _context.Shipments
                    .Include(s => s.Customer)
                    .OrderByDescending(s => s.Date)
                    .Take(5)
                    .ToListAsync(),

                // Critical Alert 1: Inventory Depletion - Finished products with stock below 20 units
                LowStockProducts = await _context.FinishedProducts
                    .Where(p => p.QuantityAvailable < 20)
                    .OrderBy(p => p.QuantityAvailable)
                    .Take(5)
                    .ToListAsync(),

                // Critical Alert 2: Material Shortage & Expiry - Ingredients with low stock (< 50kg) or near expiration
                CriticalLowStockMaterials = await _context.RawMaterials
                    .Where(m => m.QuantityInStock < 50 || m.ExpiryDate <= warningDate)
                    .OrderBy(m => m.ExpiryDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(dashboardViewModel);
        }
        catch (Exception ex)
        {
            // Technical Logging: Records the stack trace for developer debugging (e.g., DB connection issues)
            _logger.LogError(ex, "Critical failure loading Dashboard stats.");

            // Graceful Degradation: Notify the user of the partial failure without crashing the entire application
            TempData["ErrorMessage"] = $"System is busy. Some charts might not load.: {ex.Message}";

            // Return a safe, empty state to maintain UI stability
            return View(new DashboardViewModel());
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public async Task<IActionResult> About()
    {
        return View();
    }

    // Error Handling: Standard MVC error page with RequestId for support tracking
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}