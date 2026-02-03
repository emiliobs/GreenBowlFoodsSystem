using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using GreenBowlFoodsSystem.ViewModel;
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
        // Check if the user is logged in (optiona but recomended), If using Session, we might want to show data only if logged in
        // For now, we will alwayss load data to make the Dashboard look nice.
        try
        {
            var dashboardViewMdel = new DashboardViewModel
            {
                // Count batches by status (KPIs)
                ActiveBatchesCount = await _context.ProductionBatches.CountAsync(pb => pb.Status == "In Progress"),
                QAHoldCount = await _context.ProductionBatches.CountAsync(pb => pb.Status == "QA Hold"),
                PlannedCount = await _context.ProductionBatches.CountAsync(pb => pb.Status == "Planned"),

                // Count product with low stock (< 20 units)
                LowStockProductCount = await _context.ProductionBatches.CountAsync(pb => pb.QuantityProduced < 20),

                // Fech the last 5 batches (newest firt) for the activity feed
                RecentBatches = await _context.ProductionBatches
              .Include(pb => pb.FinishedProduct) // Include prodcut details (name, SKU)
              .Include(pb => pb.Supervisor) // Include supervisor username
              .OrderByDescending(pb => pb.ProductionDate)
              .Take(5)
              .ToListAsync()
            };

            return View(dashboardViewMdel); // Pass the data to the view
        }
        catch (Exception ex)
        {
            // Ilogged: record the technical error silently (for the developer), this writes dtabases connection failed,to the server console
            _logger.LogError(ex, "Critical failure loading Dashworad stats.");

            //
            TempData[""] = $"System is busy. Some charts might not load.: {ex.Message}";

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