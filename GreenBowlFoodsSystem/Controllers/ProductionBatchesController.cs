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

[Authorize]
public class ProductionBatchesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public ProductionBatchesController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        this._userManager = userManager;
    }

    // GET: ProductionBatches
    public async Task<IActionResult> Index(string searchString)
    {
        ViewData["CurrentFilter"] = searchString;
        // bring prodcut info ans supervisodr name
        var batches = _context.ProductionBatches
            .Include(p => p.FinishedProduct)
            .Include(p => p.Supervisor)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            batches = batches.Where(b => b.BatchNumber.Contains(searchString.ToLower())
                                  || b.Status.Contains(searchString.ToUpperInvariant())
                                  || b.FinishedProduct!.ProductName.Contains(searchString.ToLower()));
        }

        return View(await batches.OrderByDescending(b => b.ProductionDate).ToListAsync());
    }

    // GET: ProductionBatches/Create
    public IActionResult Create()
    {
        // Load dropdowns for products ans Supervisor (Users)
        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName");

        // Solo el Admin ve la lista de empleados
        if (User.IsInRole("Admin"))
        {
            ViewData["SupervisorId"] = new SelectList(_context.Users, "Id", "UserName");
        }

        return View();
    }

    // POST: ProductionBatches/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductionBatch productionBatch)
    {
        // Obtener usario Actual
        var currentUser = await _userManager.GetUserAsync(User);

        // Es un Staff normal (o Admin que no eligió), se lo asignamos a él mismo.
        if (currentUser != null)
        {
            productionBatch.SupervisorId = currentUser.Id;
        }

        // Evitar errores de validacion por campos que no se llenan en el formulario
        ModelState.Remove("SupervisorId");
        ModelState.Remove("Supervisor");

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

        return View(productionBatch);
    }

    // GET: ProductionBatches/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return View("NotFound");
        }

        var productionBatch = await _context.ProductionBatches.FindAsync(id);
        if (productionBatch is null)
        {
            return View("NotFound");
        }

        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName", productionBatch.FinishedProductId);

        return View(productionBatch);
    }

    // POST: ProductionBatches/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductionBatch productionBatch)
    {
        if (id != productionBatch.Id)
        {
            return View("NotFound");
        }

        // Obtener usario Actual
        var currentUser = await _userManager.GetUserAsync(User);

        // Asignar su ID
        if (currentUser != null)
        {
            productionBatch.SupervisorId = currentUser.Id;
        }

        // Evitar errores de validacion por campos que no se llenan en el formulario
        ModelState.Remove("SupervisorId");
        ModelState.Remove("Supervisor");

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
                    return View("NotFound");
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

        return View(productionBatch);
    }

    // GET: ProductionBatches/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return View("NotFound");
        }

        var productionBatch = await _context.ProductionBatches
      .Include(p => p.FinishedProduct) // Get Product Name
      .Include(p => p.Supervisor)      // Get Supervisor Name
      .Include(p => p.ProductionMaterials) //  Get the list of used ingredients
      .ThenInclude(pm => pm.RawMaterial) // Get the name of each ingredient
      .Include(p => p.ProductionStages.OrderBy(ps => ps.StartTime)) // Get the list of production stages//  Get the name of each ingredient
      .FirstOrDefaultAsync(m => m.Id == id);

        if (productionBatch is null)
        {
            return View("NotFound");
        }

        return View(productionBatch);
    }

    // GET: ProductionBatches/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return View("NotFound");
        }

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
        //  Retrieve the Batch including the Product info
        var batch = await _context.ProductionBatches
            .Include(b => b.FinishedProduct)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (batch == null)
        {
            return View("NotFound");
        }

        // Validation: Prevent closing an already closed batch
        // If we run this twice, we would duplicate inventory!
        if (batch.Status == "Completed" || batch.Status == "Cancelled")
        {
            TempData["ErrorMessage"] = "This batch is already closed.";
            return RedirectToAction(nameof(Details), new { id = batch.Id });
        }

        try
        {
            //  UPDATE INVENTORY (The Magic Moment 📈)
            // We add the produced quantity to the Finished Product stock
            if (batch.FinishedProduct != null)
            {
                batch.FinishedProduct.QuantityAvailable += batch.QuantityProduced;
                _context.Update(batch.FinishedProduct);
            }

            // CLOSE THE BATCH
            batch.Status = "Completed";

            // Optional: Capture the exact completion time if you have an EndDate column
            batch.EndDate = DateTime.Now;

            _context.Update(batch);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Batch Completed! Added {batch.QuantityProduced} units to Inventory.";
        }
        catch (Exception ex)
        {
            // If there is an error, stay on Details to show the alert
            TempData["ErrorMessage"] = $"Error finishing batch: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id = batch.Id });
        }

        // CHANGE: Redirect to Index instead of Details
        // return RedirectToAction(nameof(Index));
        return RedirectToAction(nameof(Details), new { id = batch.Id });
    }

    private bool ProductionBatchExists(int id)
    {
        return _context.ProductionBatches.Any(e => e.Id == id);
    }
}