using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace GreenBowlFoodsSystem.Controllers;

// SECURITY: Restricts access to authenticated users with Admin or Staff roles
[Authorize]
public class ReportsController : Controller
{
    // Database context field for executing analytical queries
    private readonly ApplicationDbContext _context;

    // Constructor: Injects the database context through Dependency Injection
    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Reports/Index
    // Serves as the main menu or dashboard for the different reporting modules
    public IActionResult Index()
    {
        return View();
    }

    // INVENTORY VALUATION REPORT (Asset management)
    // Calculates the monetary value of the current finished goods stock
    public async Task<IActionResult> InventoryReport()
    {
        // Querying the database for all finished products
        var inventory = await _context.FinishedProducts
            .AsNoTracking() // Performance Optimization: Disables change tracking for read-only reporting
            .OrderBy(p => p.ProductName) // Alphabetical sorting for easy identification
            .ToListAsync(); // Asynchronous execution to prevent thread blocking

        return View(inventory);
    }

    // PRODUCTION YIELD REPORT (Manufacturing Efficiency)
    // Analyzes production performance within a specific timeframe
    public async Task<IActionResult> ProductionReport(DateTime? startDate, DateTime? endDate)
    {
        // Default Logic: If no dates are selected, default to a 30-day lookback period
        var start = startDate ?? DateTime.Now.AddDays(-30);
        var end = endDate ?? DateTime.Now;

        // Eager Loading: Fetch batches including product details and supervisors for a complete audit
        var batches = await _context.ProductionBatches
            .Include(b => b.FinishedProduct) // Join with Product table
            .Include(b => b.Supervisor)      // Join with User/Staff table
            .Where(b => b.ProductionDate >= start && b.ProductionDate <= end) // Temporal filtering
            .OrderByDescending(b => b.ProductionDate) // Chronological sorting
            .AsNoTracking() // Read-only optimization
            .ToListAsync();

        // Pass filter dates back to the view for display in date picker inputs
        ViewData["StartDate"] = start.ToString("yyyy-MM-dd");
        ViewData["EndDate"] = end.ToString("yyyy-MM-dd");

        return View(batches);
    }

    // QUALITY CONTROL & SAFETY REPORT (X-Ray Checks Audit)
    // High-level audit report for food safety compliance. Tracks "Pass" vs "Fail" metrics.
    public async Task<IActionResult> QualityReport()
    {
        // Multi-level Loading: Fetch X-Ray data -> Batch -> Product metadata
        var checks = await _context.XRayChecks
            .Include(x => x.ProductionBatch)
            .ThenInclude(pb => pb.FinishedProduct) // Deep nesting to retrieve the product name
            .Include(x => x.Operator) // Identify the staff member responsible for the check
            .OrderByDescending(x => x.CheckTime)
            .AsNoTracking()
            .ToListAsync();

        // Analytical Metrics: Calculating Business Intelligence (BI) stats for the report header
        ViewBag.TotalChecks = checks.Count;
        ViewBag.FailedChecks = checks.Count(c => c.Result == "Fail");
        // Pass Rate calculation: Percentage of batches that cleared safety protocols
        ViewBag.PassRate = checks.Count > 0
            ? (double)checks.Count(c => c.Result == "Pass") / checks.Count * 100
            : 0;

        return View(checks);
    }

    // FINANCIAL SALES REPORT (Revenue tracking)
    // Extracts financial data based on realized income (paid invoices)
    public async Task<IActionResult> SalesReport()
    {
        // Fetching paid invoices to represent real cash flow
        var invoices = await _context.Invoices
            .Include(i => i.Customer) // Link each invoice to its corresponding customer
            .Where(i => i.Status == "Paid") // Business Logic: Only count confirmed revenue
            .OrderByDescending(i => i.Date)
            .AsNoTracking()
            .ToListAsync();

        // Aggregate Calculation: Summing the total revenue directly in the controller for the view
        ViewBag.TotalRevenue = invoices.Sum(i => i.TotalAmount);

        return View(invoices);
    }

    // LOGISTICS AND SHIPMENT REPORT (Outbound traceability)
    // Tracks product distribution and vehicle safety status
    public async Task<IActionResult> ShipmentReport()
    {
        // Eager Loading of the complete shipping chain
        var shipments = await _context.Shipments
            .Include(s => s.Customer) // Recipient identification
            .Include(s => s.FinishedProduct) // Product identification
            .Include(s => s.DeliveryForm) // Link to the vehicle inspection record
            .OrderByDescending(s => s.Date)
            .AsNoTracking()
            .ToListAsync();

        return View(shipments);
    }
}