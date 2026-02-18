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

    // FINANCIAL SALES REPORT (Revenue tracking by period)
    // This method calculates total revenue from paid invoices within a specific timeframe.
    public async Task<IActionResult> SalesReport(DateTime? startDate, DateTime? endDate)
    {
        // Default Logic: If no dates are provided, it defaults to the last 30 days.
        var start = startDate ?? DateTime.Now.AddDays(-30);
        var end = endDate ?? DateTime.Now;

        // Database Query: Retrieves invoices filtered by status and the selected date range.
        var invoices = await _context.Invoices
            .Include(i => i.Customer) // Eager loading to include customer metadata.
            .Where(i => i.Status == "Paid" && i.Date >= start && i.Date <= end) // Filters by specific period.
            .OrderByDescending(i => i.Date) // Sorts records from newest to oldest.
            .AsNoTracking() // Optimization for read-only data processing.
            .ToListAsync();

        // Business Intelligence: Sums the total amount of all filtered invoices.
        ViewBag.TotalRevenue = invoices.Sum(i => i.TotalAmount);

        // Persistence: Stores the selected dates in ViewData to keep them visible in the UI inputs.
        ViewData["StartDate"] = start.ToString("yyyy-MM-dd");
        ViewData["EndDate"] = end.ToString("yyyy-MM-dd");

        return View(invoices);
    }

    // LOGISTICS AND SHIPMENT REPORT (Outbound traceability by period)
    // This method tracks distribution activities and logistical value for a selected duration.
    public async Task<IActionResult> ShipmentReport(DateTime? startDate, DateTime? endDate)
    {
        // Temporal Logic: Ensures a default period is set if the user hasn't selected one.
        var start = startDate ?? DateTime.Now.AddDays(-30);
        var end = endDate ?? DateTime.Now;

        // Data Retrieval: Joins multiple tables to provide a full audit trail of shipments.
        var shipments = await _context.Shipments
            .Include(s => s.Customer) // Links the destination client.
            .Include(s => s.FinishedProduct) // Identifies the physical product dispatched.
            .Include(s => s.DeliveryForm) // Includes the mandatory vehicle safety check record.
            .Where(s => s.Date >= start && s.Date <= end) // Strict period-based filtering.
            .OrderByDescending(s => s.Date) // Chronological sorting for audit purposes.
            .AsNoTracking() // Reduces memory overhead for reporting.
            .ToListAsync();

        // UI Mapping: Passes the dates back to the view's date-picker components.
        ViewData["StartDate"] = start.ToString("MM-dd-yyyy");
        ViewData["EndDate"] = end.ToString("MM-dd-yyyy");

        return View(shipments);
    }
}