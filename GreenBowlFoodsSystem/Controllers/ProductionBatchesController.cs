using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;

namespace GreenBowlFoodsSystem.Controllers;

public class ProductionBatchesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductionBatchesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: ProductionBatches
    public async Task<IActionResult> Index()
    {
        // bring prodcut info ans supervisodr name
        var applicationDbContext = _context.ProductionBatches
            .Include(p => p.FinishedProduct)
            .Include(p => p.Supervisor);

        return View(await applicationDbContext.ToListAsync());
    }

    // GET: ProductionBatches/Create
    public IActionResult Create()
    {
        // Load dropdowns for products ans Supervisor (Users)
        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName");
        ViewData["SupervisorId"] = new SelectList(_context.Users, "Id", "Username");

        return View();
    }

    // POST: ProductionBatches/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductionBatch productionBatch)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(productionBatch);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Production batch scheduled successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error scheduling batch: {ex.Message}";
            }
        }

        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName", productionBatch.FinishedProductId);
        ViewData["SupervisorId"] = new SelectList(_context.Users, "Id", "Username", productionBatch.SupervisorId);

        return View(productionBatch);
    }

    // GET: ProductionBatches/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var productionBatch = await _context.ProductionBatches.FindAsync(id);
        if (productionBatch is null)
        {
            return NotFound();
        }

        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName", productionBatch.FinishedProductId);
        ViewData["SupervisorId"] = new SelectList(_context.Users, "Id", "Username", productionBatch.SupervisorId);

        return View(productionBatch);
    }

    // POST: ProductionBatches/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductionBatch productionBatch)
    {
        if (id != productionBatch.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(productionBatch);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Production status updated!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductionBatchExists(productionBatch.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating batch: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName", productionBatch.FinishedProductId);
        ViewData["SupervisorId"] = new SelectList(_context.Users, "Id", "Username", productionBatch.SupervisorId);

        return View(productionBatch);
    }

    // GET: ProductionBatches/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var productionBatch = await _context.ProductionBatches
      .Include(p => p.FinishedProduct) // Get Product Name
      .Include(p => p.Supervisor)      // Get Supervisor Name
      .Include(p => p.ProductionMaterials) //  Get the list of used ingredients
      .ThenInclude(pm => pm.RawMaterial) //  Get the name of each ingredient
      .FirstOrDefaultAsync(m => m.Id == id);

        if (productionBatch is null)
        {
            return NotFound();
        }

        return View(productionBatch);
    }

    // GET: ProductionBatches/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var productionBatch = await _context.ProductionBatches
            .Include(p => p.FinishedProduct)
            .Include(p => p.Supervisor)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (productionBatch is null)
        {
            return NotFound();
        }

        return View(productionBatch);
    }

    // POST: ProductionBatches/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var productionBatch = await _context.ProductionBatches.FindAsync(id);

            if (productionBatch is not null)
            {
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

    // ACTION: FINISH BATCH (Close order & Update Inventory)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinishBatch(int id)
    {
        // 1. Retrieve the Batch including the Product info
        var batch = await _context.ProductionBatches
            .Include(b => b.FinishedProduct)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (batch == null)
        {
            return NotFound();
        }

        // 2. Validation: Prevent closing an already closed batch
        // If we run this twice, we would duplicate inventory!
        if (batch.Status == "Completed" || batch.Status == "Cancelled")
        {
            TempData["ErrorMessage"] = "This batch is already closed.";
            return RedirectToAction(nameof(Details), new { id = batch.Id });
        }

        try
        {
            // 3. UPDATE INVENTORY (The Magic Moment 📈)
            // We add the produced quantity to the Finished Product stock
            if (batch.FinishedProduct != null)
            {
                batch.FinishedProduct.QuantityAvailable += batch.QuantityProduced;
                _context.Update(batch.FinishedProduct);
            }

            // 4. CLOSE THE BATCH
            batch.Status = "Completed";
            // Optional: Capture the exact completion time if you have an EndDate column
            // batch.EndDate = DateTime.Now;

            _context.Update(batch);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Batch Completed! Added {batch.QuantityProduced} units to Inventory.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error finishing batch: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = batch.Id });
    }

    private bool ProductionBatchExists(int id)
    {
        return _context.ProductionBatches.Any(e => e.Id == id);
    }
}