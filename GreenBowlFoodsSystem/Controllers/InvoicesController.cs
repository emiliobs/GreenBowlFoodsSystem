using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace GreenBowlFoodsSystem.Controllers;

[Authorize] // Enforce authentication for the entire controller
public class InvoicesController : Controller
{
    // Database context field for data access
    private readonly ApplicationDbContext _context;

    // Constructor: Assigns the injected database context to the local field
    public InvoicesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Invoices/Index
    // Handles listing, searching, and revenue calculation
    public async Task<IActionResult> Index(string searchString)
    {
        // Store the search term in ViewData to keep the text in the search input box
        ViewData["CurrentFilter"] = searchString;

        // Create an initial query including the Customer navigation property
        var invoices = _context.Invoices
            .Include(i => i.Customer)
            .AsQueryable();

        // Apply filtering logic if the search string is not null or empty
        if (!string.IsNullOrEmpty(searchString))
        {
            // Filter by Invoice Number or Customer Name using a contains search
            invoices = invoices.Where(i => i.InvoiceNumber.Contains(searchString)
                                        || i.Customer!.CustomerName.Contains(searchString));
        }

        // BI Calculation: Sum the TotalAmount of all filtered invoices for the UI header
        ViewBag.TotalRevenue = await invoices.SumAsync(i => i.TotalAmount);

        // Execute query, order by date (newest first), and convert to a list asynchronously
        return View(await invoices.OrderByDescending(i => i.Date).ToListAsync());
    }

    // GET: Invoices/Details/5
    // Fetches a single invoice with all its related product items
    public async Task<IActionResult> Details(int? id)
    {
        // Safety check: If id is null, return a custom NotFound view
        if (id == null)
        {
            return View("NotFound");
        }

        // Fetch invoice with deep loading: Customer -> Items -> FinishedProduct
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Items) // Load the collection of line items
            .ThenInclude(fp => fp.FinishedProduct) // Join with product catalog to get names
            .FirstOrDefaultAsync(m => m.Id == id);

        // If no record is found in the database, return the NotFound view
        if (invoice == null)
        {
            return View("NotFound");
        }

        // Pass the fully loaded invoice object to the view
        return View(invoice);
    }

    // GET: Invoices/Create
    // Prepares the form with an auto-generated invoice number
    public IActionResult Create()
    {
        // Populate the dropdown list with customer names for the selection menu
        ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CustomerName");

        // Logic to calculate the next ID based on current record count
        var nextId = _context.Invoices.Count() + 1;
        // Format string to create a professional serial (e.g., INV-2026-005)
        var nextInvoiceNumber = $"INV-{DateTime.Now.Year}-{nextId:D3}";

        // Initialize a new Invoice object with default business values
        var newInvoice = new Invoice
        {
            InvoiceNumber = nextInvoiceNumber,
            Date = DateTime.UtcNow, // Use UTC for standardized time tracking
            Status = "Unpaid" // Default status for any new billing record
        };

        // Send the pre-filled object to the creation form
        return View(newInvoice);
    }

    // POST: Invoices/Create
    // Validates and persists the new invoice record
    [HttpPost]
    [ValidateAntiForgeryToken] // Security: Guard against Cross-Site Request Forgery
    public async Task<IActionResult> Create(Invoice invoice)
    {
        try
        {
            // Manually remove navigation properties from validation to avoid false errors
            ModelState.Remove("Customer");
            ModelState.Remove("Items");

            // Check if the provided model data meets the requirements
            if (ModelState.IsValid)
            {
                // Add the new entity to the context tracker
                _context.Add(invoice);
                // Execute the SQL INSERT command asynchronously
                await _context.SaveChangesAsync();

                // Set success message for SweetAlert notification
                TempData["SuccessMessage"] = "Invoice created successfully!";
                // Redirect back to the list view
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            // Catch database or logic errors and pass the message to the UI alert
            TempData["ErrorMessage"] = $"Error creating invoice: {ex.Message}";
        }

        // Re-populate the customer dropdown if validation fails and we return to the form
        ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CustomerName", invoice.CustomerId);

        // Return to the view with the current data to show validation errors
        return View(invoice);
    }

    // GET: Invoices/Edit/5
    // Retrieves the invoice data for the modification form
    public async Task<IActionResult> Edit(int? id)
    {
        // Null check for safety
        if (id == null)
        {
            return View("NotFound");
        }

        // Find the specific invoice record by its primary key
        var invoice = await _context.Invoices.FindAsync(id);

        // Return NotFound if the ID does not exist in the database
        if (invoice == null)
        {
            return View("NotFound");
        }

        // Load customers for the dropdown, selecting the current customer as default
        ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CustomerName", invoice.CustomerId);
        return View(invoice);
    }

    // POST: Invoices/Edit/5
    // Saves modified data to the database
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Invoice invoice)
    {
        // Verify that the ID in the URL matches the ID in the posted data
        if (id != invoice.Id)
        {
            return View("NotFound");
        }

        // Check model state validation
        if (ModelState.IsValid)
        {
            try
            {
                // Mark the entity as modified for Entity Framework
                _context.Update(invoice);
                // Execute the SQL UPDATE command
                await _context.SaveChangesAsync();

                // Success notification
                TempData["SuccessMessage"] = "Invoice updated successfully!";
            }
            catch (Exception ex)
            {
                // Failure notification
                TempData["ErrorMessage"] = $"Error updating invoice: {ex.Message}";
            }

            // Return to the main list
            return RedirectToAction(nameof(Index));
        }

        // Re-populate dropdown in case of validation failure
        ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CustomerName", invoice.CustomerId);
        return View(invoice);
    }

    // GET: Invoices/Delete/5
    // Shows a confirmation screen before permanent removal
    public async Task<IActionResult> Delete(int? id)
    {
        // ID safety check
        if (id == null)
        {
            return View("NotFound");
        }

        // Load the invoice and the customer name for the confirmation UI
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(m => m.Id == id);

        // Record exists check
        if (invoice == null)
        {
            return View("NotFound");
        }

        return View(invoice);
    }

    // POST: Invoices/Delete/5
    // Executes the actual deletion from the database
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            // Retrieve the record from the database
            var invoice = await _context.Invoices.FindAsync(id);

            // If the record exists, remove it from the tracker
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
            }

            // Execute the SQL DELETE command
            await _context.SaveChangesAsync();
            // Success notification for the UI
            TempData["SuccessMessage"] = "Invoice deleted successfully!";
        }
        catch (Exception ex)
        {
            // Error handling for foreign key constraints or DB errors
            TempData["ErrorMessage"] = $"Error deleting invoice: {ex.Message}";
        }

        // Redirect to the index list
        return RedirectToAction(nameof(Index));
    }

    // Helper method: Checks if an invoice exists in the database by its ID
    private bool InvoiceExists(int id)
    {
        return _context.Invoices.Any(e => e.Id == id);
    }
}