using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GreenBowlFoodsSystem.Controllers;

[Authorize] // Restricts access to authenticated users only
public class ProductionBatchesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    // Constructor: Dependency Injection of the Database Context and Identity User Manager
    public ProductionBatchesController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        this._userManager = userManager;
    }

    // GET: ProductionBatches
    // Lists all production orders with search and eager loading for related data
    public async Task<IActionResult> Index(string searchString)
    {
        // Persistence: Maintain the current search string in the view's search input
        ViewData["CurrentFilter"] = searchString;

        // Eager Loading: Fetch related FinishedProduct and Supervisor (User) information
        var batches = _context.ProductionBatches
            .Include(p => p.FinishedProduct)
            .Include(p => p.Supervisor)
            .AsQueryable();

        // Search Logic: Filter by Batch Number, Status, or Finished Product Name
        if (!string.IsNullOrEmpty(searchString))
        {
            batches = batches.Where(b => b.BatchNumber.Contains(searchString.ToLower())
                                  || b.Status.Contains(searchString.ToUpperInvariant())
                                  || b.FinishedProduct!.ProductName.Contains(searchString.ToLower()));
        }

        // Return the ordered list asynchronously (newest production dates first)
        return View(await batches.OrderByDescending(b => b.ProductionDate).ToListAsync());
    }

    // GET: ProductionBatches/Create
    // Prepares the form to schedule a new production batch
    public IActionResult Create()
    {
        // Populate the dropdown list for Finished Products
        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName");

        // Role-Based Logic: Only Admins can manually select a different supervisor from the list
        if (User.IsInRole("Admin"))
        {
            ViewData["SupervisorId"] = new SelectList(_context.Users, "Id", "UserName");
        }

        return View();
    }

    // POST: ProductionBatches/Create
    // Validates and saves the new production batch into the system
    [HttpPost]
    [ValidateAntiForgeryToken] // Security: Prevents CSRF attacks
    public async Task<IActionResult> Create(ProductionBatch productionBatch)
    {
        // Audit Trail: Retrieve the currently logged-in user
        var currentUser = await _userManager.GetUserAsync(User);

        // Ownership Logic: Automatically assign the current user as supervisor if not otherwise specified
        if (currentUser != null)
        {
            productionBatch.SupervisorId = currentUser.Id;
        }

        // Validation Cleaning: Remove navigation properties from validation to avoid false model state errors
        ModelState.Remove("SupervisorId");
        ModelState.Remove("Supervisor");

        if (ModelState.IsValid)
        {
            try
            {
                // Track the new record in the database context
                _context.Add(productionBatch);
                // Execute the SQL INSERT command
                await _context.SaveChangesAsync();

                // Success Feedback: Store message for the next request's SweetAlert
                TempData["SuccessMessage"] = "Production batch scheduled successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Error Handling: Notify the user about database or processing errors
                TempData["ErrorMessage"] = $"Error scheduling batch: {ex.Message}";
            }
        }

        // Reload data if validation fails to return to the form safely
        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName", productionBatch.FinishedProductId);

        return View(productionBatch);
    }

    // GET: ProductionBatches/Edit/5
    // Retrieves existing batch data for modification
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return View("NotFound");
        }

        // Find the batch by its primary key
        var productionBatch = await _context.ProductionBatches.FindAsync(id);
        if (productionBatch is null)
        {
            return View("NotFound");
        }

        // Populate dropdown with the currently associated product selected
        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName", productionBatch.FinishedProductId);

        return View(productionBatch);
    }

    // POST: ProductionBatches/Edit/5
    // Processes status or details updates for an existing production order
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductionBatch productionBatch)
    {
        // Route safety: Verify that the ID in the URL matches the object ID
        if (id != productionBatch.Id)
        {
            return View("NotFound");
        }

        // Audit Trail: Re-verify user and maintain supervisor assignment
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser != null)
        {
            productionBatch.SupervisorId = currentUser.Id;
        }

        ModelState.Remove("SupervisorId");
        ModelState.Remove("Supervisor");

        if (ModelState.IsValid)
        {
            try
            {
                // Update the entity state to 'Modified'
                _context.Update(productionBatch);
                // Commit changes to the database
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Production status updated!";
            }
            catch (DbUpdateConcurrencyException)
            {
                // Concurrency Logic: Check if the record still exists if the update fails
                if (!ProductionBatchExists(productionBatch.Id))
                {
                    return View("NotFound");
                }
                else
                {
                    throw; // Re-throw the exception for general error handling
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating batch: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName", productionBatch.FinishedProductId);

        return View(productionBatch);
    }

    // GET: ProductionBatches/Details/5
    // Comprehensive View: Shows everything from ingredients used to production timelines
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return View("NotFound");
        }

        // Multi-level Loading (Eager Loading): Retrieve the complete tree for traceability
        var productionBatch = await _context!.ProductionBatches!
            .Include(p => p.FinishedProduct!) // Get product details (SKU, Name)
            .Include(p => p.Supervisor!)      // Get supervisor's name
            .Include(p => p.ProductionMaterials!) // Get junction table for used ingredients
                .ThenInclude(pm => pm.RawMaterial) // Join to get ingredient specific names
            .Include(p => p.ProductionStages.OrderBy(ps => ps.StartTime)) // Load time stages in sequence
            .FirstOrDefaultAsync(m => m.Id == id);

        if (productionBatch is null)
        {
            return View("NotFound");
        }

        return View(productionBatch);
    }

    // GET: ProductionBatches/Delete/5
    // Display confirmation screen. Access restricted to Administrator role.
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return View("NotFound");
        }

        // Load record with associations to show exactly what is being deleted
        var productionBatch = await _context.ProductionBatches
            .Include(p => p.FinishedProduct)
            .Include(p => p.Supervisor)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (productionBatch is null)
        {
            return View("NotFound");
        }

        return View(productionBatch);
    }

    // POST: ProductionBatches/Delete/5
    // Executes the permanent removal of the production record
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            // Fetch the record from the database
            var productionBatch = await _context.ProductionBatches.FindAsync(id);

            if (productionBatch is not null)
            {
                // Remove the entity and commit changes
                _context.ProductionBatches.Remove(productionBatch);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Batch record deleted.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error Deleting batch: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    // ACTION: FINISH BATCH
    // Operational Logic: Finalizes production and performs automatic inventory inbound
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinishBatch(int id)
    {
        // Load the batch with the associated product to allow inventory updates
        var batch = await _context.ProductionBatches
            .Include(b => b.FinishedProduct)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (batch == null)
        {
            return View("NotFound");
        }

        // Safety Validation: Prevent duplicate inventory increases if the batch is already finalized
        if (batch.Status == "Completed" || batch.Status == "Cancelled")
        {
            TempData["ErrorMessage"] = "This batch is already closed.";
            return RedirectToAction(nameof(Details), new { id = batch.Id });
        }

        try
        {
            // INVENTORY SYNCHRONIZATION: The manufactured quantity is added to the Finished Goods stock
            if (batch.FinishedProduct != null)
            {
                batch.FinishedProduct.QuantityAvailable += batch.QuantityProduced;
                _context.Update(batch.FinishedProduct); // Track the inventory change
            }

            // Status Update: Mark the order as 'Completed'
            batch.Status = "Completed";

            // Audit Logic: Record the precise moment the manufacturing process ended
            batch.EndDate = DateTime.Now;

            // Commit both the Batch update and the Product update in a single transaction
            _context.Update(batch);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Batch Completed! Added {batch.QuantityProduced} units to Inventory.";
        }
        catch (Exception ex)
        {
            // In case of error, redirect back to details with a technical message
            TempData["ErrorMessage"] = $"Error finishing batch: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id = batch.Id });
        }

        // Stay on the details view to show the final state of the completed batch
        return RedirectToAction(nameof(Details), new { id = batch.Id });
    }

    // Helper: Internal check to verify entity existence and handle database consistency
    private bool ProductionBatchExists(int id)
    {
        return _context.ProductionBatches.Any(e => e.Id == id);
    }
}