using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // Added for FindAsync

namespace GreenBowlFoodsSystem.Controllers
{
    [Authorize]
    public class ProductionMaterialsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductionMaterialsController(ApplicationDbContext context)
        {
            // Dependency Injection: Requesting access to the Database
            _context = context;
        }

        // GET: ProductionMaterial/Create
        public async Task<IActionResult> Create(int? batchId)
        {
            // validation: If no Batch ID is provided, we can't link the ingredient
            if (batchId is null)
            {
                return NotFound();
            }

            // Use Include to get the Product Name
            var batch = await _context.ProductionBatches
                .Include(b => b.FinishedProduct) // Join to Prodcuts table
                .FirstOrDefaultAsync(m => m.Id == batchId);

            if (batch == null)
            {
                return NotFound();
            }

            // Pass the Batch ID to the view (so we know who the parent is)
            ViewData["ProductionBatchId"] = batchId;
            ViewData["ProductionBatchNumber"] = batch!.BatchNumber;
            ViewData["ProductName"] = batch.FinishedProduct!.ProductName ?? "unkhown Product";

            // Load the dropdown list with all available ingredients
            ViewData["RawMaterialId"] = new SelectList(_context.RawMaterials, "Id", "MaterialName");

            return View();
        }

        // POST: ProductionMaterial/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductionMaterial productionMaterial)
        {
            // We ignore these validations because the HTML form only sends numeric IDs,
            // not the full Objects (ProductionBatch/RawMaterial).
            ModelState.Remove("ProductionBatch");
            ModelState.Remove("RawMaterial");

            if (ModelState.IsValid)
            {
                try
                {
                    var rawMaterialInDb = await _context.RawMaterials.FindAsync(productionMaterial.RawMaterialId);

                    if (rawMaterialInDb != null)
                    {
                        // LOGIC FIX: Compare quantities, not ModelState again
                        if (rawMaterialInDb.QuantityInStock < productionMaterial.QuantityUsed)
                        {
                            // Stock Error: SweetAlert Error popup
                            TempData["ErrorMessage"] = $"Not enough stock! We only have {rawMaterialInDb.QuantityInStock} {rawMaterialInDb.Unit}";

                            // Also keep field validation error so the form shows red text
                            ModelState.AddModelError("QuantityUsed", "Insufficient stock available");
                        }
                        else
                        {
                            // Deduct from Warehouse (Inventory Management)
                            rawMaterialInDb.QuantityInStock -= productionMaterial.QuantityUsed;

                            // Register the Usage(Traceability)
                            _context.Add(productionMaterial);

                            // Mark the raw material as modified
                            _context.Update(rawMaterialInDb);

                            // Commit changes to SQL Server
                            await _context.SaveChangesAsync();

                            // Success: Trigger SweetAlert success Popup
                            TempData["SuccessMessage"] = "Ingredient added and Inventory updated!";

                            // Redirect back to the Batch Details
                            return RedirectToAction("Details", "ProductionBatches", new { id = productionMaterial.ProductionBatchId });
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Raw Material not found in the database.";
                    }
                }
                catch (Exception ex)
                {
                    // Catch technical errors (e.g. Database connection lost)
                    TempData["ErrorMessage"] = $"Error creating production material: {ex.Message}";
                }
            }
            else
            {
                // Validation Error (e.g. empty fields)
                TempData["ErrorMessage"] = "Please check the form for errors.";
            }

            // If we get here (error), we need to RE-FETCH the batch info
            // otherwise, the Read-Only fields in the view will be empty/broken.
            var batchInfo = await _context.ProductionBatches
                .Include(b => b.FinishedProduct)
                .FirstOrDefaultAsync(b => b.Id == productionMaterial.ProductionBatchId);

            if (batchInfo != null)
            {
                ViewData["ProductionBatchNumber"] = batchInfo.BatchNumber;
                ViewData["ProductName"] = batchInfo.FinishedProduct?.ProductName;
            }

            // Reload view if failed
            // TYPO FIX: Keys must match the GET method exactly
            ViewData["ProductionBatchId"] = productionMaterial.ProductionBatchId;
            ViewData["RawMaterialId"] = new SelectList(_context.RawMaterials, "Id", "MaterialName", productionMaterial.RawMaterialId);

            return View(productionMaterial);
        }
    }
}