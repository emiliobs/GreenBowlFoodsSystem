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
    private readonly ApplicationDbContext _context; // Add the database context
    private readonly ILogger<HomeController> _logger;

    // Inject the contex into the constructor
    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        this._context = context;
        this._logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        // Check if the user is logged in (optiona but recomended),
        // If using Session, we might want to show data only if logged in
        if (!User.Identity!.IsAuthenticated)
        {
            return View(new DashboardViewModel());
        }

        // IF LOGGED IN: Load the Dashboard Data
        try
        {
            // KPI: Total Revenue (From Invoices module)
            var revenue = await _context.Invoices.SumAsync(i => i.TotalAmount);

            //  KPI: Total Inventory Value (Money)
            var totalValue = await _context.FinishedProducts
                .SumAsync(p => p.UnitPrice * p.QuantityAvailable);

            // KPI: Active Shipments (Not Delivered)
            var activeShipments = await _context.Shipments
                .CountAsync(s => s.Status != "Delivered");

            //  KPI: Quality Issues (Last 24h)
            var yesterday = DateTime.Now.AddHours(-24);
            var failedChecks = await _context.XRayChecks
                .CountAsync(x => x.Result == "Fail" && x.CheckTime >= yesterday);

            //  KPI: Expiry Risk (Next 7 Days)
            var warningDate = DateTime.Now.AddDays(7);
            var expiringCount = await _context.RawMaterials
                .CountAsync(m => m.ExpiryDate <= warningDate);

            // Build Viewmodel
            var dashboardViewModel = new DashboardViewModel
            {
                TotalInventoryValue = totalValue,
                TotalRevenue = revenue,
                ActiveShipmentsCount = activeShipments,
                QualityIssuesToday = failedChecks,
                ExpiringSoonCount = expiringCount,

                // Feed 1: Recent Batches
                RecentBatches = await _context.ProductionBatches
                    .Include(pb => pb.FinishedProduct)
                    .OrderByDescending(pb => pb.ProductionDate)
                    .Take(5)
                    .ToListAsync(),

                // Feed 2: Recent Shipments (NEW!)
                RecentShipments = await _context.Shipments
                    .Include(s => s.Customer)
                    .OrderByDescending(s => s.Date)
                    .Take(5)
                    .ToListAsync(),

                // Alert 1: Low Stock Products (< 20 units)
                LowStockProducts = await _context.FinishedProducts
                    .Where(p => p.QuantityAvailable < 20)
                    .OrderBy(p => p.QuantityAvailable)
                    .Take(5)
                    .ToListAsync(),

                // Alert 2: Low Stock Ingredients (< 50kg) OR Expiring Soon (NEW!)
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
            // Ilogged: record the technical error silently (for the developer), this writes dtabases connection failed,to the server console
            _logger.LogError(ex, "Critical failure loading Dashworad stats.");

            // Show a user-friendly message on the dashboard, but don't crash the app.
            // This way, the user knows something went wrong, but we don't expose technical details.
            TempData["ErrorMessage"] = $"System is busy. Some charts might not load.: {ex.Message}";

            // Return the view anyway (maybe empty) so the app doesn't crash completely
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}