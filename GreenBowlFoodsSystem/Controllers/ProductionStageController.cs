using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenBowlFoodsSystem.Controllers;

[Authorize]
public class ProductionStageController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductionStageController(ApplicationDbContext context)
    {
        this._context = context;
    }

    // GET: ProductionStage/Create?batchId=5
    // We pass the batchId as a query parameter to associate the new stage with the correct production batch
    public IActionResult Create(int? batchId)
    {
        if (batchId == null)
        {
            return View("NotFound");
        }

        try
        {
            // Pre-populate the ProductionStage model with the provided batchId in the form
            var stage = new ProductionStage
            {
                ProductionBatchId = batchId.Value,
                StartTime = DateTime.Now // Set the start time to the current time by default
            };

            return View(stage);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred while preparing the form. Please try again: {ex.Message}";
        }

        return View();
    }

    // POST: ProductionStage/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductionStage productionStage)
    {
        try
        {
            if (ModelState.IsValid)
            {
                _context.Add(productionStage);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Production stage created successfully.";

                // Redirect to the details page of the associated production batch to show the new stage in context
                return RedirectToAction("Details", "ProductionBatches", new { id = productionStage.ProductionBatchId });
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error creating ProductionStage {ex.Message}";
        }

        // If we got this far, something failed; redisplay the form with error messages
        return View(productionStage);
    }

    // GET: ProductionStage/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return View("NotFound");
        }

        try
        {
            var prodcutioStage = await _context.ProductionStages.FindAsync(id);
            if (prodcutioStage is null)
            {
                return View("NotFound");
            }

            return View(prodcutioStage);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error find prodcutio stage: {ex.Message}";
        }

        return View();
    }

    // POST: ProductionStage/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductionStage productionStage)
    {
        if (id != productionStage.Id)
        {
            return View("NotFound");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(productionStage);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Production stage updated successfully.";
                // Redirect to the details page of the associated production batch to show the updated stage in context
                return RedirectToAction("Details", "ProductionBatches", new { id = productionStage.ProductionBatchId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error editing production sateg: {ex.Message}";
            }
        }

        return View(productionStage);
    }
}