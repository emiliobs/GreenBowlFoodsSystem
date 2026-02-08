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
    private readonly ApplicationDbContext _context;

    public ReceivingFormController(ApplicationDbContext context)
    {
        this._context = context;
    }

    // GET: ReceivingForm
    public async Task<IActionResult> Index()
    {
        // We include supplier and user to show names in the view instead of just IDs
        var receivingforms = await _context.ReceivingForms
            .Include(rf => rf.Supplier)
            .Include(rf => rf.ReceivedBy)
            .Include(rf => rf.RawMaterial)
            .OrderByDescending(rf => rf.Date) // show newest receipts first.
            .ToListAsync();

        return View(receivingforms);
    }

    // GET: Show Create Form
    public async Task<IActionResult> Create()
    {
        // Pupulate dropdowns for suppliers and raw Materials
        ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName");
        ViewData["RawMaterialId"] = new SelectList(_context.RawMaterials, "Id", "MaterialName");

        return View();
    }

    [HttpPost] // Process the receip
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> Create(ReceivingForm receivingForm)
    {
        try
        {
            // 1. Logic: Automatically assign the logged-in user
            // Using "User.Identity?.Name" prevents potential null reference if Identity is not fully loaded
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);
            receivingForm.ReceivedById = user?.Id ?? 0;

            // 2. Logic: Set default date if missing
            if (receivingForm.Date == DateTime.MinValue)
            {
                receivingForm.Date = DateTime.Now;
            }

            // 3. Validation Check
            if (ModelState.IsValid)
            {
                // Now we access properties directly from the objects
                if (receivingForm.IsAccepted)
                {
                    var rawMaterial = await _context.RawMaterials.FindAsync(receivingForm);

                    if (rawMaterial != null)
                    {
                        // Increase stock (update)
                        rawMaterial.QuantityInStock += receivingForm.QuantityReceived;
                        _context.Update(rawMaterial);

                        // Success message
                        // Note: Included "C" format string for Currency in TotalAmount
                        TempData["SuccessMessage"] = $"Success! Added {receivingForm.QuantityReceived} {rawMaterial.Unit} of {rawMaterial.MaterialName}. " +
                                                     $"Cost: {receivingForm.TotalAmount}";
                    }
                }
                else
                {
                    // Rejection message (Stock remains unchanged)
                    TempData["ErrorMessage"] = "Receipt saved as REJECTED. Inventory was NOT updated.";
                }

                // C. Commit changes to Database
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            // add a visible error th the form if something goes wrong during processing
            ModelState.AddModelError("", "An unexpected error cocurred while saving. Please try again.");
            TempData["ErroMessage"] = $"Error creating receipt.: {ex.Message}";
        }

        //Reload dropdowns if validation fails
        ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName");
        ViewData["RawMaterialId"] = new SelectList(_context.RawMaterials, "Id", "MaterialName");

        return View();
    }

    // GET: Show details of a receipt
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return View("NotFound");
        }

        // We use .Include() to eager load the related tables,
        // this ensure we can display Supplier Name and operator Name
        var receivingform = await _context.ReceivingForms
            .Include(rf => rf.Supplier)
            .Include(rf => rf.ReceivedBy)
            .Include(rf => rf.RawMaterial)
            .FirstOrDefaultAsync(rf => rf.Id == id);

        if (receivingform is null)
        {
            return View("NotFound");
        }

        return View(receivingform);
    }
}