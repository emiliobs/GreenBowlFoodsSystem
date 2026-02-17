using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenBowlFoodsSystem.Controllers;

[Authorize] // Restricts access to authenticated users only
public class DeliveryFormsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    // Constructor: Dependency Injection of the Database Context and Identity User Manager
    public DeliveryFormsController(ApplicationDbContext context, UserManager<User> userManager)
    {
        this._context = context;
        this._userManager = userManager;
    }

    // GET: DeliveryForms
    // Displays the log of outbound delivery checks (Vehicle inspection).
    public async Task<IActionResult> Index(string searchString)
    {
        // Persistence: Save the current search filter to ViewData to maintain UI state
        ViewData["CurrentFilter"] = searchString;

        // Initialize query with Eager Loading to include the User who approved the form
        var forms = _context.DeliveryForms
            .Include(d => d.ApprovedBy)
            .AsQueryable();

        // Apply Server-Side Filtering based on user input
        if (!string.IsNullOrEmpty(searchString))
        {
            // Multi-field search: Trailer Number, Driver Name, or Approver's Username
            forms = forms.Where(d => d.TrailerNumber!.Contains(searchString.ToLower())
                                  || d.DriverName!.Contains(searchString.ToLower())
                                  || d.ApprovedBy!.UserName!.Contains(searchString.ToLower()));
        }

        // Execution: Sort by date descending so the most recent inspections appear first
        return View(await forms.OrderByDescending(d => d.CheckDate).ToListAsync());
    }

    // GET: DeliveryForms/Details/5
    // Shows full inspection details and all shipments loaded onto the specific vehicle
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return View("NotFound");
        }

        var deliveryForm = await _context.DeliveryForms
            .Include(df => df.ApprovedBy)
            .FirstOrDefaultAsync(df => df.Id == id);

        if (deliveryForm == null)
        {
            return View("NotFound");
        }

        // Business Logic: Load and display all shipments associated with this specific delivery vehicle
        ViewBag.RelatedShipments = await _context.Shipments
            .Include(s => s.Customer)
            .Include(s => s.FinishedProduct)
            .Where(s => s.DeliveryFormId == id)
            .ToListAsync();

        return View(deliveryForm);
    }

    // GET: DeliveryForms/Create
    // Prepares data for a new inspection, filtering for shipments ready for dispatch
    public IActionResult Create()
    {
        // UI Logic: Fetch only "Pending" shipments that haven't been assigned to a truck yet
        var pendingShipments = _context.Shipments
               .Include(s => s.Customer)
               .Where(s => s.DeliveryFormId == null && s.Status != "Cancelled")
               .ToList();

        // Populate MultiSelectList for the view's shipment selection UI
        ViewBag.PendingShipments = new MultiSelectList(pendingShipments, "Id", "TrackingNumber");

        return View();
    }

    // POST: DeliveryForms/Create
    // Saves the inspection form and updates the status of all selected shipments
    [HttpPost]
    [ValidateAntiForgeryToken] // Security: Prevents Cross-Site Request Forgery attacks
    public async Task<IActionResult> Create(DeliveryForm deliveryForm, int[] selectedShipmentIds)
    {
        // Audit Trail: Automatically assign the currently logged-in user as the Approver
        var currentUser = await _userManager.GetUserAsync(User);
        deliveryForm.ApprovedById = currentUser?.Id ?? 0;

        try
        {
            if (ModelState.IsValid)
            {
                // Transactional Logic: 1. Save the Master record (DeliveryForm)
                _context.Add(deliveryForm);
                await _context.SaveChangesAsync();

                // 2. Update child records (Shipments) to link them to the truck and update status
                if (selectedShipmentIds != null && selectedShipmentIds.Length > 0)
                {
                    var shipmentsToUpdate = await _context.Shipments
                         .Where(s => selectedShipmentIds.Contains(s.Id))
                         .ToListAsync();

                    foreach (var shipment in shipmentsToUpdate)
                    {
                        shipment.DeliveryFormId = deliveryForm.Id;
                        shipment.Status = "Shipped"; // Status transition to 'Shipped' upon vehicle assignment
                        _context.Update(shipment);
                    }

                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Delivery form created successfully.";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            // Error Handling: Log and notify user of database exceptions
            TempData["ErrorMessagge"] = $"Error creating Delivery: {ex.Message}";
        }

        return View(deliveryForm);
    }

    // GET: DeliveryForms/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return View("NotFound");
        }

        var deliveryForm = await _context.DeliveryForms.FindAsync(id);

        if (deliveryForm == null)
        {
            return View("NotFound");
        }

        return View(deliveryForm);
    }

    // POST: DeliveryForms/Edit/5
    // Updates inspection data (e.g., correcting trailer numbers or driver names)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DeliveryForm deliveryForm)
    {
        if (id != deliveryForm.Id)
        {
            return View("NotFound");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(deliveryForm);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Delivery form updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating Delivery: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
        return View(deliveryForm);
    }

    // GET: DeliveryForms/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return View("NotFound");
        }

        var deliveryform = await _context.DeliveryForms
            .Include(df => df.ApprovedBy)
            .FirstOrDefaultAsync(df => df.Id == id);

        if (deliveryform is null)
        {
            return View("NotFound");
        }

        return View(deliveryform);
    }

    // POST: DeliveryForms/Delete/5
    // Safely removes a form after unlinking associated shipments
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deliveryForm = await _context.DeliveryForms.FindAsync(id);

        if (deliveryForm != null)
        {
            // Data Integrity Logic: Unlink Shipments before deleting the parent form.
            // This prevents foreign key constraint issues and ensures shipments aren't lost.
            var linkedShipenments = await _context.Shipments.Where(s => s.DeliveryFormId == id).ToListAsync();

            // Status Rollback: Return shipments to 'Pending' status so they can be reassigned to a new truck
            foreach (var shipment in linkedShipenments)
            {
                shipment.DeliveryFormId = null;
                shipment.Status = "Pending";
                _context.Update(shipment);
            }

            // Final step: Delete the delivery form once children are safe
            _context.DeliveryForms.Remove(deliveryForm);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Delivery form deleted successfully.";
        }

        return RedirectToAction(nameof(Index));
    }
}