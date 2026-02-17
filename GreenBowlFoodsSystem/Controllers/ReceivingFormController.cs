using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace GreenBowlFoodsSystem.Controllers;

[Authorize] // Only logged-in users can see this page
public class ReceivingFormController : Controller
{
    // Database context field for data persistence operations
    private readonly ApplicationDbContext _context;

    // Constructor: Dependency Injection of the ApplicationDbContext to interact with the SQL database
    public ReceivingFormController(ApplicationDbContext context)
    {
        this._context = context;
    }

    // GET: ReceivingForms
    // Displays the log of received raw materials with filtering and pagination.
    public async Task<IActionResult> Index(string searchString)
    {
        // Persistence: Store current filter to keep it in the search box after page reload
        ViewData["CurrentFilter"] = searchString;

        // Initialize query with Eager Loading for related entities to prevent N+1 query problems
        // We include RawMaterial, Supplier, and the User who received it to show names instead of IDs
        var forms = _context.ReceivingForms
            .Include(r => r.RawMaterial)
            .Include(r => r.Supplier)
            .Include(r => r.ReceivedBy)
            .AsQueryable();

        //  Server side Filtering: Apply search filters if the user provided criteria
        if (!string.IsNullOrEmpty(searchString))
        {
            // Case-insensitive filtering by Material Name, Supplier Name, or Trailer Number
            forms = forms.Where(r => r.RawMaterial!.MaterialName.Contains(searchString.ToLower())
                                  || r.Supplier!.SupplierName.Contains(searchString.ToLower())
                                  || r.TrailerNumber!.Contains(searchString.ToLower()));
        }

        //  Execution: Sort by Date (newest first) to ensure chronological traceability
        return View(await forms.OrderByDescending(r => r.Date).ToListAsync());
    }

    // GET: Show Create Form
    // Prepares the environment for a new material receipt record
    public async Task<IActionResult> Create()
    {
        // Populate DropdownLists for Suppliers and Raw Materials to feed the UI select menus
        ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName");
        ViewData["RawMaterialId"] = new SelectList(_context.RawMaterials, "Id", "MaterialName");

        return View();
    }

    // POST: Process the receipt
    // Handles the core business logic for material arrival and inventory updates
    [HttpPost]
    [AutoValidateAntiforgeryToken] // Security: Protect against CSRF attacks
    public async Task<IActionResult> Create(ReceivingForm receivingForm)
    {
        try
        {
            //  Logic: Automatically assign the identity of the user performing the receipt
            // Fetching the user from the database using the identity name from the current session
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);
            receivingForm.ReceivedById = user?.Id ?? 0;

            // Data Integrity: Set default system date/time if no specific date was provided in the form
            if (receivingForm.Date == DateTime.MinValue)
            {
                receivingForm.Date = DateTime.Now;
            }

            // Model Validation: Verify if the submitted form data meets the data annotation requirements
            if (ModelState.IsValid)
            {
                // Business Rule: Only increment inventory if the material shipment was accepted after inspection
                if (receivingForm.IsAccepted)
                {
                    // Look up the specific RawMaterial record in the database
                    var rawMaterial = await _context.RawMaterials.FindAsync(receivingForm.RawMaterialId);

                    if (rawMaterial != null)
                    {
                        // STOCK UPDATE: Add the newly received quantity to the current warehouse stock levels
                        rawMaterial.QuantityInStock += receivingForm.QuantityReceived;

                        // Mark the raw material entity as modified for the EF tracking system
                        _context.Update(rawMaterial);

                        // UI Feedback: Create a successful notification string with summary data
                        TempData["SuccessMessage"] = $"Success! Added {receivingForm.QuantityReceived} {rawMaterial.Unit} of {rawMaterial.MaterialName}. " +
                                                     $"Cost: {receivingForm.TotalAmount}";
                    }
                }
                else
                {
                    // Quality Alert: If rejected, we save the record for audit but DO NOT update inventory
                    TempData["ErrorMessage"] = "Receipt saved as REJECTED. Inventory was NOT updated.";
                }

                // TRANSACTION COMMIT: Save both the ReceivingForm record and the RawMaterial stock update simultaneously
                await _context.SaveChangesAsync();

                // Redirect back to the history log
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            // Exception Handling: Catch database or logic errors and notify the UI without crashing
            ModelState.AddModelError("", "An unexpected error cocurred while saving. Please try again.");
            TempData["ErroMessage"] = $"Error creating receipt.: {ex.Message}";
        }

        // Re-populate DropdownLists if validation fails to prevent UI rendering errors
        ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName");
        ViewData["RawMaterialId"] = new SelectList(_context.RawMaterials, "Id", "MaterialName");

        return View();
    }

    // GET: Show details of a specific receipt
    // Provides a full view of a single transaction including supplier and inspector metadata
    public async Task<IActionResult> Details(int? id)
    {
        // Safety check for null ID parameters
        if (id is null)
        {
            return View("NotFound");
        }

        // Eager Loading Implementation: Ensures all related data is fetched in a single efficient query
        // This allows the display of the Supplier Name, Operator Name, and Material Name in the view
        var receivingform = await _context.ReceivingForms
            .Include(rf => rf.Supplier)
            .Include(rf => rf.ReceivedBy)
            .Include(rf => rf.RawMaterial)
            .FirstOrDefaultAsync(rf => rf.Id == id);

        // Validation: Verify if the record exists in the database
        if (receivingform is null)
        {
            return View("NotFound");
        }

        // Return the fully hydrated model to the details view
        return View(receivingform);
    }
}