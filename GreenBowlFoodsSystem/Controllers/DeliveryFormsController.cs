using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenBowlFoodsSystem.Controllers;

public class DeliveryFormsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public DeliveryFormsController(ApplicationDbContext context, UserManager<User> userManager)
    {
        this._context = context;
        this._userManager = userManager;
    }

    // GET: DeliveryForms
    public async Task<IActionResult> Index()
    {
        return View(await _context.DeliveryForms.Include(df => df.ApprovedBy).ToListAsync());
    }

    // GET: DeliveryForms/Details/5
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

        // Return the details view with the delivery form data ( load the shipments associated with this delivery)
        ViewBag.RelatedShipments = await _context.Shipments
            .Include(s => s.Customer)
            .Include(s => s.FinishedProduct)
            .Where(s => s.DeliveryFormId == id)
            .ToListAsync();

        return View(deliveryForm);
    }

    // GET: DeliveryForms/Create
    public IActionResult Create()
    {
        // We need show a list of Pending shipments that  don't have a deliveryForm yet,
        // so the user can select which shipments to include in this delivery form

        var pendingShipments = _context.Shipments
               .Include(s => s.Customer)
               .Where(s => s.DeliveryFormId == null && s.Status != "Cancelled")
               .ToList();

        ViewBag.PendingShipments = new MultiSelectList(pendingShipments, "Id", "TrackingNumber");

        return View();
    }

    // POST: DeliveryForms/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DeliveryForm deliveryForm, int[] selectedShipmentIds)
    {
        // Auto-assing Approver (Logged User)
        var currentUser = await _userManager.GetUserAsync(User);
        deliveryForm.ApprovedById = currentUser?.Id ?? 0;

        try
        {
            if (ModelState.IsValid)
            {
                // Save delivery form first to get aan ID
                _context.Add(deliveryForm);
                await _context.SaveChangesAsync();

                // Link selected shipments to this delivery form
                if (selectedShipmentIds != null && selectedShipmentIds.Length > 0)
                {
                    var shipmentsToUpdate = await _context.Shipments
                         .Where(s => selectedShipmentIds.Contains(s.Id))
                         .ToListAsync();

                    foreach (var shipment in shipmentsToUpdate)
                    {
                        shipment.DeliveryFormId = deliveryForm.Id;
                        shipment.Status = "Shipped"; // Update status to Shipped when included in a delivery form
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
                TempData["ErrorMessage"] = $"Error deleting Delivey: {ex.Message}";
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

        // Load the delivery form with approver details for confirmation
        var deliveryform = await _context.DeliveryForms
            .Include(df => df.ApprovedBy) // Include Approver details for confirmation
            .FirstOrDefaultAsync(df => df.Id == id);
        if (deliveryform is null)
        {
            return View("NotFound");
        }

        return View(deliveryform);
    }

    // POST: DeliveryForms/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deliveryForm = await _context.DeliveryForms.FindAsync(id);

        if (deliveryForm != null)
        {
            //  Unlink Shipments before deleting the form, We find all shipments currently on this truck.
            var linkedShipenments = await _context.Shipments.Where(s => s.DeliveryFormId == id).ToListAsync();

            // we unload  them (set back to pendig)
            foreach (var shipment in linkedShipenments)
            {
                shipment.DeliveryFormId = null; // Remove link to delivery form
                shipment.Status = "Pending"; // Reset status to Pending when unlinked from a delivery form
                _context.Update(shipment);
            }

            // Now it is safe to delete the delivery form
            _context.DeliveryForms.Remove(deliveryForm);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Delivery form deleted successfully.";
        }

        return RedirectToAction(nameof(Index));
    }
}