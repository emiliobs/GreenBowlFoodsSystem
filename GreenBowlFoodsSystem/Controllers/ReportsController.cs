using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace GreenBowlFoodsSystem.Controllers
{
    // SECURTY: Only accessible by logged-in users (Admin & Staff)
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // INVENTORY VALUATION REPORT (Money sitting in warehouse)
        // Shows quantity, unit price, and total value of stock.
        public async Task<IActionResult> InventoryReport()
        {
            var inventory = await _context.FinishedProducts
                .AsNoTracking() // Performance optimization for read-only
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            return View(inventory);
        }

        // 3. PRODUCTION YIELD REPORT (Efficiency)
        // Shows completed batches, downtime, and quantity produced vs target.
        public async Task<IActionResult> ProductionReport(DateTime? startDate, DateTime? endDate)
        {
            // Default to current month if no dates provided
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;

            var batches = await _context.ProductionBatches
                .Include(b => b.FinishedProduct)
                .Include(b => b.Supervisor)
                .Where(b => b.ProductionDate >= start && b.ProductionDate <= end)
                .OrderByDescending(b => b.ProductionDate)
                .AsNoTracking()
                .ToListAsync();

            ViewData["StartDate"] = start.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = end.ToString("yyyy-MM-dd");

            return View(batches);
        }

        // 4. QUALITY CONTROL & SAFETY REPORT (X-Ray Checks)
        // Critical for food safety audits. Shows Passes vs Fails.
        public async Task<IActionResult> QualityReport()
        {
            var checks = await _context.XRayChecks
                .Include(x => x.ProductionBatch)
                .ThenInclude(pb => pb.FinishedProduct)
                .Include(x => x.Operator)
                .OrderByDescending(x => x.CheckTime)
                .AsNoTracking()
                .ToListAsync();

            // Simple Metrics for the View Header
            ViewBag.TotalChecks = checks.Count;
            ViewBag.FailedChecks = checks.Count(c => c.Result == "Fail");
            ViewBag.PassRate = checks.Count > 0
                ? (double)checks.Count(c => c.Result == "Pass") / checks.Count * 100
                : 0;

            return View(checks);
        }

        // 5. FINANCIAL SALES REPORT (Revenue)
        // Shows paid invoices and total revenue generated.
        public async Task<IActionResult> SalesReport()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .Where(i => i.Status == "Paid") // Only count real money
                .OrderByDescending(i => i.Date)
                .AsNoTracking()
                .ToListAsync();

            // Calculate Grand Total in Controller (or View)
            ViewBag.TotalRevenue = invoices.Sum(i => i.TotalAmount);

            return View(invoices);
        }

        // 6. LOGISTICS & SHIPMENT REPORT
        // Shows what went out, to whom, and carrier details.
        public async Task<IActionResult> ShipmentReport()
        {
            var shipments = await _context.Shipments
                .Include(s => s.Customer)
                .Include(s => s.FinishedProduct)
                .Include(s => s.DeliveryForm) // Linked Vehicle Check
                .OrderByDescending(s => s.Date)
                .AsNoTracking()
                .ToListAsync();

            return View(shipments);
        }
    }
}