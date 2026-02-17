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

[Authorize] // Enforce authentication: Only logged-in users can manage raw materials
public class RawMaterialsController : Controller
{
    // Dependency Injection: Access the database context to perform CRUD operations
    private readonly ApplicationDbContext _context;

    // Constructor: Injects the ApplicationDbContext into the local field
    public RawMaterialsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: RawMaterials
    // Displays the current inventory levels and expiry dates for all raw materials.
    public async Task<IActionResult> Index(string searchString)
    {
        // Persistence: Save the current search filter to the View to keep it in the search box
        ViewData["CurrentFilter"] = searchString;

        // Eager Loading: Initialize the query and include Supplier navigation data
        var rawMaterials = _context.RawMaterials
            .Include(r => r.Supplier)
            .AsQueryable();

        // Search Logic: Apply filters if the user provided search criteria
        if (!string.IsNullOrEmpty(searchString))
        {
            // Perform case-insensitive search by Material Name, Lot Number, or Supplier Name
            rawMaterials = rawMaterials.Where(s => s.MaterialName.Contains(searchString.ToLower())
                                                || s.LotNumber.Contains(searchString.ToLower())
                                                || s.Supplier!.SupplierName.Contains(searchString.ToLower()));
        }

        // Execution: Order by ExpiryDate (Ascending) to highlight items nearing expiration first
        return View(await rawMaterials.OrderBy(r => r.ExpiryDate).ToListAsync());
    }

    // GET: RawMaterials/Create
    // Renders the form to add a new raw material to the system
    public IActionResult Create()
    {
        // Data Preparation: Populate the Dropdown List for suppliers using SelectList
        ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName");
        return View();
    }

    // POST: RawMaterials/Create
    // Receives the form data and saves the new material record to the database.
    [HttpPost]
    [ValidateAntiForgeryToken] // Security measure: Prevent Cross-Site Request Forgery (CSRF)
    public async Task<IActionResult> Create([Bind("Id,MaterialName,LotNumber,QuantityInStock,Unit,ExpiryDate,SupplierId")] RawMaterial rawMaterial)
    {
        // Validation: Verify that the incoming data meets the Model requirements
        if (ModelState.IsValid)
        {
            try
            {
                // Track the new entity in the database context
                _context.Add(rawMaterial);
                // Commit changes asynchronously to SQL Server
                await _context.SaveChangesAsync();

                // Success Message: Triggers the green SweetAlert notification in the UI
                TempData["SuccessMessage"] = "Materia added to inventory!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Exception Management: Capture and display errors if database operation fails
                TempData["ErrorMessage"] = $"Error adding material: {ex.Message}";
            }
        }

        // Fallback: If validation fails, reload the Supplier Dropdown to prevent UI errors
        ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName", rawMaterial.SupplierId);

        return View(rawMaterial);
    }

    // GET: RawMaterials/Edit/5
    // Fetches an existing material record by ID for modification
    public async Task<IActionResult> Edit(int? id)
    {
        // Safety check for null ID
        if (id == null)
        {
            return View("NotFound");
        }

        // Data Retrieval: Search for the material in the database by Primary Key
        var rawMaterial = await _context.RawMaterials.FindAsync(id);
        if (rawMaterial == null)
        {
            return NotFound();
        }

        // Data Preparation: Reload the supplier Dropdown, pre-selecting the current supplier
        ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName", rawMaterial.SupplierId);

        return View(rawMaterial);
    }

    // POST: RawMaterials/Edit/5
    // Updates an existing record with new data from the form
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,MaterialName,LotNumber,QuantityInStock,Unit,ExpiryDate,SupplierId")] RawMaterial rawMaterial)
    {
        // Route protection: Ensure URL ID matches the hidden field ID in the model
        if (id != rawMaterial.Id)
        {
            return View("NotFound");
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Mark the entity as modified to trigger an UPDATE query
                _context.Update(rawMaterial);
                await _context.SaveChangesAsync();

                // Notification: Trigger success SweetAlert
                TempData["SuccessMessage"] = "Material details updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                // Concurrency Logic: Verify if the record still exists if the update fails
                if (!RawMaterialExists(rawMaterial.Id))
                {
                    return View("NotFound");
                }
                else
                {
                    throw; // Re-throw the original exception for logging
                }
            }
            catch (Exception ex)
            {
                // Error Notification: Trigger red SweetAlert
                ViewData["ErroMessage"] = $"Error updating material: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Re-populate data in case of validation failure
        ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName", rawMaterial.SupplierId);

        return View(rawMaterial);
    }

    // GET: RawMaterials/Details/5
    // Shows detailed information and the linked supplier for a specific material
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // Eager Loading: Fetch the record including Supplier metadata
        var rawMaterial = await _context.RawMaterials
            .Include(r => r.Supplier)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (rawMaterial == null)
        {
            return View("NotFound");
        }

        return View(rawMaterial);
    }

    // GET: RawMaterials/Delete/5
    // Display confirmation screen. Access restricted to Admin role for security.
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return View("NotFound");
        }

        // Load record with Supplier info for clear identification in the confirmation screen
        var rawMaterial = await _context.RawMaterials
            .Include(r => r.Supplier)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (rawMaterial == null)
        {
            return View("NotFound");
        }

        return View(rawMaterial);
    }

    // POST: RawMaterials/Delete/5
    // Permanently removes the material record from the inventory database
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            // Find the record by ID before attempting removal
            var rawMaterial = await _context.RawMaterials.FindAsync(id);
            if (rawMaterial != null)
            {
                // Remove the entity from context tracking
                _context.RawMaterials.Remove(rawMaterial);
            }

            // Execute SQL DELETE command
            await _context.SaveChangesAsync();

            // Notification: Triggers success SweetAlert
            TempData["SuccessMessage"] = "Material deleted from inventory!";
        }
        catch (Exception ex)
        {
            // Error feedback for the user
            TempData["ErrorMessage"] = $"Error deleting material: {ex.Message}";
        }

        // Final redirect to the list view
        return RedirectToAction(nameof(Index));
    }

    // Helper method: Internal check to verify if a material exists by ID
    private bool RawMaterialExists(int id)
    {
        return _context.RawMaterials.Any(e => e.Id == id);
    }
}