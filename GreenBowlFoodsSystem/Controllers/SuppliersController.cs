using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GreenBowlFoodsSystem.Controllers;

[Authorize] // Access is restricted to authenticated users (Admin and Staff)
public class SuppliersController : Controller
{
    // Database context field for executing data persistence operations
    private readonly ApplicationDbContext _context;

    // Constructor: Dependency Injection of the ApplicationDbContext
    public SuppliersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Suppliers
    // Displays the directory of approved raw material suppliers with search capabilities.
    public async Task<IActionResult> Index(string searchString)
    {
        // Persistence: Save the current search filter to ViewData to maintain UI state in the search box
        ViewData["CurrentFilter"] = searchString;

        // Initialize the query as IQueryable to allow efficient server-side filtering
        var suppliers = _context.Suppliers.AsQueryable();

        // Apply Search Filter if the user provided search criteria in the UI
        if (!string.IsNullOrEmpty(searchString))
        {
            // Case-insensitive filtering by Supplier Name, Contact Person, or Email Address
            suppliers = suppliers.Where(s => s.SupplierName!.Contains(searchString.ToLower())
                                          || s.ContactPerson!.Contains(searchString.ToLower())
                                          || s.Email!.Contains(searchString.ToLower()));
        }

        // Execute query: Order results by Supplier Name (Alphabetical) and return the list asynchronously
        return View(await suppliers.OrderBy(s => s.SupplierName).ToListAsync());
    }

    // GET: Suppliers/Details/5
    // Retrieves and displays metadata for a single supplier record
    public async Task<IActionResult> Details(int? id)
    {
        // Safety check for null ID parameters in the request URL
        if (id == null)
        {
            return View("NotFound");
        }

        // Fetch the specific supplier record using the primary key
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(m => m.Id == id);

        // Validation check to ensure the requested supplier exists in the database
        if (supplier == null)
        {
            return View("NotFound");
        }

        // Return the supplier entity to the details view
        return View(supplier);
    }

    // GET: Suppliers/Create
    // Renders the registration form for adding a new business partner
    public IActionResult Create()
    {
        return View();
    }

    // POST: Suppliers/Create
    // Validates and persists a new supplier entry into the database
    [HttpPost]
    [ValidateAntiForgeryToken] // Security: Protect against Cross-Site Request Forgery (CSRF)
    public async Task<IActionResult> Create(Supplier supplier)
    {
        // Check if the submitted data meets the Model's data annotation requirements
        if (ModelState.IsValid)
        {
            try
            {
                // Track the new entity in the context
                _context.Add(supplier);
                // Execute the SQL INSERT command asynchronously
                await _context.SaveChangesAsync();

                // Success Feedback: Set message for the SweetAlert notification system
                TempData["SuccessMessage"] = "New supplier registered successfully!";

                // Redirect back to the supplier list
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Error Handling: Catch database exceptions and notify the user
                TempData["ErrorMessage"] = $"Error registering supplier: {ex.Message}";
            }
        }

        // If validation fails, return to the form with the current data to show errors
        return View(supplier);
    }

    // GET: Suppliers/Edit/5
    // Fetches an existing record to be modified in the update form
    public async Task<IActionResult> Edit(int? id)
    {
        // ID parameter safety check
        if (id == null)
        {
            return View("NotFound");
        }

        // Find the record by ID
        var supplier = await _context.Suppliers.FindAsync(id);

        // Return custom NotFound view if the record is missing
        if (supplier == null)
        {
            return View("NotFound");
        }

        return View(supplier);
    }

    // POST: Suppliers/Edit/5
    // Processes update requests for existing supplier metadata
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Supplier supplier)
    {
        // Security check: Ensure the URL ID parameter matches the model ID
        if (id != supplier.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Mark entity as Modified to trigger an UPDATE query
                _context.Update(supplier);
                await _context.SaveChangesAsync();

                // Success Notification
                TempData["SuccessMessage"] = "Supplier details updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                // Concurrency Logic: Verify record existence if an update conflict occurs
                if (!SupplierExists(supplier.Id))
                {
                    TempData["ErrorMessage"] = "Supplier not fount, it might have been deleted.";
                    return NotFound();
                }
                else
                {
                    // Re-throw if it is a different technical error
                    throw;
                }
            }
            catch (Exception ex)
            {
                // General Exception feedback
                TempData["ErrorMessage"] = $"Could not update supplier. Try again: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Fallback to the form if validation is failed
        return View(supplier);
    }

    // GET: Suppliers/Delete/5
    // Display confirmation screen. Access is restricted to Administrator role for security.
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // Fetch the record to be identified on the confirmation screen
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(m => m.Id == id);

        if (supplier == null)
        {
            return NotFound();
        }

        return View(supplier);
    }

    // POST: Suppliers/Delete/5
    // Executes the actual removal of the supplier from the database
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            // Find the entity by ID
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier != null)
            {
                // Remove from context tracking
                _context.Suppliers.Remove(supplier);
            }

            // Execute SQL DELETE command
            await _context.SaveChangesAsync();

            // Success feedback
            TempData["SuccessMessage"] = "Supplier removed from the system.";
        }
        catch (Exception ex)
        {
            // Catch-all: Prevents deletion if there are foreign key constraints (e.g., existing raw materials)
            TempData["ErrorMessahe"] = $"Cannot delete supplier. It may have related records: {ex.Message}";
        }

        // Final redirect to the supplier directory
        return RedirectToAction(nameof(Index));
    }

    // Helper: Internal check to verify entity existence using Any() for performance
    private bool SupplierExists(int id)
    {
        return _context.Suppliers.Any(e => e.Id == id);
    }
}