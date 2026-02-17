using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenBowlFoodsSystem.Controllers;

[Authorize] // Enforce authentication: Only authorized users can manage material consumption
public class ProductionMaterialsController : Controller
{
    private readonly ApplicationDbContext _context;

    // Constructor: Dependency Injection of the Database Context to handle persistence
    public ProductionMaterialsController(ApplicationDbContext context)
    {
        // Dependency Injection: Requesting access to the Database service
        _context = context;
    }

    // GET: ProductionMaterial/Create
    // Prepares the interface to link an ingredient to a specific production batch
    public async Task<IActionResult> Create(int? batchId)
    {
        // Validation: If no Batch ID is provided, the record cannot be linked to a parent entity
        if (batchId is null)
        {
            return View("NotFound");
        }

        // Database Query: Use Eager Loading (Include) to fetch the related Finished Product details
        var batch = await _context.ProductionBatches
            .Include(b => b.FinishedProduct) // SQL JOIN: Retrieve Product information through navigation property
            .FirstOrDefaultAsync(m => m.Id == batchId);

        // Security check: Ensure the requested batch exists in the database
        if (batch == null)
        {
            return View("NotFound");
        }

        // Pass metadata to the view via ViewData to display read-only context to the user
        ViewData["ProductionBatchId"] = batchId; // Foreign key for the parent batch
        ViewData["ProductionBatchNumber"] = batch!.BatchNumber; // Display batch serial number
        ViewData["ProductName"] = batch.FinishedProduct!.ProductName ?? "unknown Product"; // Display output product name

        // Data Retrieval: Load the dropdown list with all available raw materials (ingredients)
        ViewData["RawMaterialId"] = new SelectList(_context.RawMaterials, "Id", "MaterialName");

        return View();
    }

    // POST: ProductionMaterial/Create
    // Processes the usage logic, inventory deduction, and data persistence
    [HttpPost]
    [ValidateAntiForgeryToken] // Security: Prevents Cross-Site Request Forgery (CSRF) attacks
    public async Task<IActionResult> Create(ProductionMaterial productionMaterial)
    {
        // Model Sanitization: Remove full navigation objects from validation state
        // The HTML form only sends numeric Foreign Keys, not complex objects.
        ModelState.Remove("ProductionBatch");
        ModelState.Remove("RawMaterial");

        // Validate the incoming data against the model constraints
        if (ModelState.IsValid)
        {
            try
            {
                // Business Logic: Fetch the current stock of the selected raw material from the database
                var rawMaterialInDb = await _context.RawMaterials.FindAsync(productionMaterial.RawMaterialId);

                if (rawMaterialInDb != null)
                {
                    // INVENTORY VALIDATION: Check if enough stock exists before allowing usage
                    if (rawMaterialInDb.QuantityInStock < productionMaterial.QuantityUsed)
                    {
                        // Stock Error Feedback: Trigger a SweetAlert error via TempData
                        TempData["ErrorMessage"] = $"Not enough stock! We only have {rawMaterialInDb.QuantityInStock} {rawMaterialInDb.Unit}";

                        // Field Validation: Add a visual error message directly to the form field
                        ModelState.AddModelError("QuantityUsed", "Insufficient stock available");
                    }
                    else
                    {
                        // INVENTORY MANAGEMENT: Deduct the used quantity from the physical warehouse stock
                        rawMaterialInDb.QuantityInStock -= productionMaterial.QuantityUsed;

                        // TRACEABILITY: Register the material usage linked to the specific production batch
                        _context.Add(productionMaterial);

                        // Persistence Update: Explicitly mark the raw material entity as modified
                        _context.Update(rawMaterialInDb);

                        // DATABASE TRANSACTION: Commit both changes (Inventory Deduction + Usage Record) to SQL Server
                        await _context.SaveChangesAsync();

                        // Success Feedback: Trigger a SweetAlert success popup for the user
                        TempData["SuccessMessage"] = "Ingredient added and Inventory updated!";

                        // Navigation: Redirect back to the parent Batch Details view to see updated materials list
                        return RedirectToAction("Details", "ProductionBatches", new { id = productionMaterial.ProductionBatchId });
                    }
                }
                else
                {
                    // Error Handling: Handle scenarios where a material ID is invalid or missing
                    TempData["ErrorMessage"] = "Raw Material not found in the database.";
                }
            }
            catch (Exception ex)
            {
                // Technical Exception Handling: Catch connection issues or SQL constraint failures
                TempData["ErrorMessage"] = $"Error creating production material: {ex.Message}";
            }
        }
        else
        {
            // Validation Error Feedback: Alert the user if the form contains malformed data
            TempData["ErrorMessage"] = "Please check the form for errors.";
        }

        // FALLBACK LOGIC: If an error occurred, re-fetch parent batch info to prevent broken UI fields
        var batchInfo = await _context.ProductionBatches
            .Include(b => b.FinishedProduct)
            .FirstOrDefaultAsync(b => b.Id == productionMaterial.ProductionBatchId);

        if (batchInfo != null)
        {
            // Restore contextual view data for the user
            ViewData["ProductionBatchNumber"] = batchInfo.BatchNumber;
            ViewData["ProductName"] = batchInfo.FinishedProduct?.ProductName;
        }

        // Re-populate view requirements before returning the user to the form
        ViewData["ProductionBatchId"] = productionMaterial.ProductionBatchId;
        ViewData["RawMaterialId"] = new SelectList(_context.RawMaterials, "Id", "MaterialName", productionMaterial.RawMaterialId);

        return View(productionMaterial);
    }
}