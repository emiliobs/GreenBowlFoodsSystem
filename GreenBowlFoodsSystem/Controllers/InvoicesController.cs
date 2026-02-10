using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;

namespace GreenBowlFoodsSystem.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Invoices
        public async Task<IActionResult> Index()
        {
            return View(await _context.Invoices.Include(i => i.Customer).ToListAsync());
        }

        // GET: Invoices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }

            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Items) // Load the items/products associated with the invoice
                .ThenInclude(fp => fp.FinishedProduct)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (invoice == null)
            {
                return View("NotFound");
            }

            return View(invoice);
        }

        // GET: Invoices/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CustomerName");

            // Auto-generate the next invoice number, e.g., "INV-0001", "INV-0002", etc.
            var nextId = _context.Invoices.Count() + 1;
            var nextInvoiceNumber = $"INV-{DateTime.Now.Year}-{nextId:D3}"; // Format: INV-2024-001, INV-2024-002, etc.

            var newInvoice = new Invoice
            {
                InvoiceNumber = nextInvoiceNumber,
                Date = DateTime.UtcNow, // Set the default date to today
                Status = "Unpaid" // Set the default status to "Unpaid"
            };

            return View(newInvoice);
        }

        // POST: Invoices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Invoice invoice)
        {
            try
            {
                // Remove validation for items/Customer as we only send IDs from the form
                ModelState.Remove("Customer");
                ModelState.Remove("Items");

                if (ModelState.IsValid)
                {
                    _context.Add(invoice);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Invoice created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating invoice: {ex.Message}";
            }

            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CustomerName", invoice.CustomerId);

            return View(invoice);
        }

        // GET: Invoices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }

            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return View("NotFound");
            }

            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CustomerName", invoice.CustomerId);
            return View(invoice);
        }

        // POST: Invoices/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Invoice invoice)
        {
            if (id != invoice.Id)
            {
                return View("NotFound");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invoice);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Invoice updated successfully!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating invoice: {ex.Message}";
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CustomerName", invoice.CustomerId);
            return View(invoice);
        }

        // GET: Invoices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }

            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (invoice == null)
            {
                return View("NotFound");
            }

            return View(invoice);
        }

        // POST: Invoices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var invoice = await _context.Invoices.FindAsync(id);

                if (invoice != null)
                {
                    _context.Invoices.Remove(invoice);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Invoice deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting invoice: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool InvoiceExists(int id)
        {
            return _context.Invoices.Any(e => e.Id == id);
        }
    }
}