using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenBowlFoodsSystem.Controllers;

[Authorize] // Enforces authentication to ensure only authorized staff can record production progress
public class ProductionStageController : Controller
{
    private readonly ApplicationDbContext _context;

    // Constructor: Injects the ApplicationDbContext via Dependency Injection for data persistence
    public ProductionStageController(ApplicationDbContext context)
    {
        this._context = context;
    }

    // GET: ProductionStage/Create?batchId=5
    // Prepares the interface to add a new operational step to a specific manufacturing batch
    public IActionResult Create(int? batchId)
    {
        // Safety check: The system requires a valid Batch ID to link the stage to a parent process
        if (batchId == null)
        {
            return View("NotFound");
        }

        try
        {
            // Business Logic: Initialize the ProductionStage model with the linked Batch ID
            // StartTime is set to the current system time to simplify data entry for the operator
            var stage = new ProductionStage
            {
                ProductionBatchId = batchId.Value,
                StartTime = DateTime.Now
            };

            return View(stage);
        }
        catch (Exception ex)
        {
            // Error Handling: Capture technical failures and notify the user through TempData alerts
            TempData["ErrorMessage"] = $"An error occurred while preparing the form. Please try again: {ex.Message}";
        }

        return View();
    }

    // POST: ProductionStage/Create
    // Validates and persists the new production step into the database
    [HttpPost]
    [ValidateAntiForgeryToken] // Security: Guard against Cross-Site Request Forgery (CSRF)
    public async Task<IActionResult> Create(ProductionStage productionStage)
    {
        try
        {
            // Server-side validation check against model data annotations
            if (ModelState.IsValid)
            {
                // Track the new entity in the database context
                _context.Add(productionStage);
                // Execute the SQL INSERT command asynchronously to maintain server responsiveness
                await _context.SaveChangesAsync();

                // Success Feedback: Trigger a SweetAlert notification for the user
                TempData["SuccessMessage"] = "Production stage created successfully.";

                // Traceability Navigation: Redirect back to the parent batch details to maintain context
                return RedirectToAction("Details", "ProductionBatches", new { id = productionStage.ProductionBatchId });
            }
        }
        catch (Exception ex)
        {
            // Exception Management: Inform the user about database or model errors
            TempData["ErrorMessage"] = $"Error creating ProductionStage {ex.Message}";
        }

        // Fallback: If validation fails, redisplay the form with existing input and error messages
        return View(productionStage);
    }

    // GET: ProductionStage/Edit/5
    // Retrieves a specific stage record for updates (e.g., adding an End Time or Notes)
    public async Task<IActionResult> Edit(int? id)
    {
        // Null parameter safety check
        if (id is null)
        {
            return View("NotFound");
        }

        try
        {
            // Fetch the specific stage by its Primary Key
            var prodcutioStage = await _context.ProductionStages.FindAsync(id);

            // Validation: Ensure the record actually exists in the database
            if (prodcutioStage is null)
            {
                return View("NotFound");
            }

            return View(prodcutioStage);
        }
        catch (Exception ex)
        {
            // Error Handling for database retrieval failures
            TempData["ErrorMessage"] = $"Error find prodcutio stage: {ex.Message}";
        }

        return View();
    }

    // POST: ProductionStage/Edit/5
    // Processes modifications to the production stage (e.g., finishing a cook cycle)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductionStage productionStage)
    {
        // Route Protection: Ensure the URL ID matches the hidden field ID in the model
        if (id != productionStage.Id)
        {
            return View("NotFound");
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Mark the entity state as Modified for Entity Framework tracking
                _context.Update(productionStage);
                // Execute the SQL UPDATE command
                await _context.SaveChangesAsync();

                // Success Feedback via TempData (SweetAlert)
                TempData["SuccessMessage"] = "Production stage updated successfully.";

                // Navigation Logic: Return the user to the Production Batch Details dashboard
                return RedirectToAction("Details", "ProductionBatches", new { id = productionStage.ProductionBatchId });
            }
            catch (Exception ex)
            {
                // Catch-all for data update exceptions
                TempData["ErrorMessage"] = $"Error editing production sateg: {ex.Message}";
            }
        }

        // Re-display the view with error context if ModelState is invalid
        return View(productionStage);
    }
}