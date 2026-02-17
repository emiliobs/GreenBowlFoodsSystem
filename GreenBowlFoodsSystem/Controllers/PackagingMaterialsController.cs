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

[Authorize] // Restricts access to this controller to authenticated users only
public class PackagingMaterialsController : Controller
{
    // Database context field for data persistence operations
    private readonly ApplicationDbContext _context;

    // Constructor: Injects the database context into the controller via Dependency Injection
    public PackagingMaterialsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: PackagingMaterials
    // Displays the inventory of packaging items (boxes, labels, etc.) with filtering and sorting
    public async Task<IActionResult> Index(string searchString)
    {
        // Persistence: Save the search filter to ViewData to maintain UI state in the search box
        ViewData["CurrentFilter"] = searchString;

        // 1. Initialize query with eager loading (Include Supplier navigation property)
        // Using IQueryable to allow further filtering before executing the SQL command
        var packaging = _context.PackagingMaterials
            .Include(p => p.Supplier)
            .AsQueryable();

        // 2. Apply Search Filter if the user provided search criteria
        if (!string.IsNullOrEmpty(searchString))
        {
            // Perform case-insensitive search by Material Name or Supplier Name
            packaging = packaging.Where(p => p.MaterialName.Contains(searchString.ToLower())
                                          || p.Supplier!.SupplierName.Contains(searchString.ToLower()));
        }

        // 3. Execute query: Order by QuantityInStock (Ascending) to highlight low stock items first
        // This sorting serves as a primitive alert system for replenishment
        return View(await packaging.OrderBy(p => p.QuantityInStock).ToListAsync());
    }

    // GET: PackagingMaterials/Details/5
    // Retrieves a single material's metadata along with its associated supplier info
    public async Task<IActionResult> Details(int? id)
    {
        // Safety check for null ID parameters
        if (id == null)
        {
            return View("NotFound");
        }

        // Fetch record using Eager Loading to avoid the N+1 query problem
        var packagingMaterial = await _context.PackagingMaterials
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(m => m.Id == id);

        // Validation check if the requested ID exists in the database
        if (packagingMaterial == null)
        {
            return View("NotFound");
        }

        // Return the material entity to the details view
        return View(packagingMaterial);
    }

    // GET: PackagingMaterials/Create
    // Prepares the form for a new entry by populating the Supplier selection list
    public IActionResult Create()
    {
        // Populate ViewData with a SelectList of suppliers to feed the dropdown menu
        ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName");
        return View();
    }

    // POST: PackagingMaterials/Create
    // Validates and saves a new packaging material to the database
    [HttpPost]
    [ValidateAntiForgeryToken] // Security: Prevents Cross-Site Request Forgery (CSRF)
    public async Task<IActionResult> Create(PackagingMaterial packagingMaterial)
    {
        try
        {
            // Server-side validation check
            if (ModelState.IsValid)
            {
                // Track the new entity in the context
                _context.Add(packagingMaterial);
                // Execute the SQL INSERT command asynchronously
                await _context.SaveChangesAsync();

                // Success Feedback: Store message in TempData for SweetAlert notification
                TempData["SuccessMessage"] = "Packaging material created successfully!";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            // Error Handling: Capture database exceptions and notify the user
            TempData["ErrorMessage"] = $"Error creating packaging material: {ex.Message}";
        }

        // If validation or creation fails, re-populate the Supplier list and return to form
        ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName", packagingMaterial.SupplierId);
        return View(packagingMaterial);
    }

    // GET: PackagingMaterials/Edit/5
    // Retrieves existing data to be modified in the edit form
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return View("NotFound");
        }

        // Find record by primary key
        var packagingMaterial = await _context.PackagingMaterials.FindAsync(id);

        if (packagingMaterial == null)
        {
            return View("NotFound");
        }

        // Pre-select the current supplier in the dropdown menu
        ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName", packagingMaterial.SupplierId);
        return View(packagingMaterial);
    }

    // POST: PackagingMaterials/Edit/5
    // Processes the update request for a specific packaging material record
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PackagingMaterial packagingMaterial)
    {
        // Verification: Ensure the URL ID matches the hidden field ID in the model
        if (id != packagingMaterial.Id)
        {
            return View("NotFound");
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Mark entity as Modified for Entity Framework tracking
                _context.Update(packagingMaterial);
                // Execute SQL UPDATE command
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Packaging material updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // UI Alert: Inform user about failure to update the database
                TempData["ErrorMessage"] = $"Error updating packaging material: {ex.Message}";
            }
        }

        // Re-populate dropdown list if state is invalid
        ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName", packagingMaterial.SupplierId);
        return View(packagingMaterial);
    }

    // GET: PackagingMaterials/Delete/5
    // Displays a confirmation view before permanent deletion
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return View("NotFound");
        }

        // Load the record with Supplier info for clear identification in the UI
        var packagingMaterial = await _context.PackagingMaterials
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (packagingMaterial == null)
        {
            return View("NotFound");
        }

        return View(packagingMaterial);
    }

    // POST: PackagingMaterials/Delete/5
    // Executes the removal of the material record from the database
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            // Retrieve the record to be removed
            var packagingMaterial = await _context.PackagingMaterials.FindAsync(id);
            if (packagingMaterial != null)
            {
                // Remove from context tracking
                _context.PackagingMaterials.Remove(packagingMaterial);
            }

            // Execute SQL DELETE command
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Packaging material deleted successfully!";
        }
        catch (Exception ex)
        {
            // Exception Handling: Important for cases where foreign keys block deletion
            TempData["ErrorMessage"] = $"Error deleting packaging material: {ex.Message}";
        }

        // Redirect to the main inventory list
        return RedirectToAction(nameof(Index));
    }

    // Helper: Verify if a record exists using the ID (used for concurrency checks)
    private bool PackagingMaterialExists(int id)
    {
        return _context.PackagingMaterials.Any(e => e.Id == id);
    }
}