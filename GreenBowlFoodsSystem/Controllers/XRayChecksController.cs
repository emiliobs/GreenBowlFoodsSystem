using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenBowlFoodsSystem.Controllers;

[Authorize]
public class XRayChecksController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public XRayChecksController(ApplicationDbContext context, UserManager<User> userManager)
    {
        this._context = context;
        this._userManager = userManager;
    }

    // GET: XRayChecks (LIst all inspections)
    public async Task<IActionResult> Index()
    {
        try
        {
            // We use .Include() to eager load the related tables, this ensure we can display Batch NUmber and operator Name
            // instead of just showing IDs like "1" or "5".
            var getOperatorAndproductioBatch = await _context.XRayChecks
                .Include(x => x.Operator)
                .Include(x => x.ProductionBatch)
                .ToListAsync();

            return View(getOperatorAndproductioBatch);
        }
        catch (Exception ex)
        {
            // log the error (optionnal) and redirec or show an error view.
            ViewData["ErrorMessage"] = $"Error loading index: {ex.Message}";
            return View(new List<XRayCheck>()); // Return emptylist to prevent crash.
        }
    }

    //GET: XRayChecks/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return View("NotFound");
        }

        try
        {
            var xRayCheks = await _context.XRayChecks
                .Include(x => x.Operator)
                .Include(x => x.ProductionBatch)
                .FirstOrDefaultAsync(x => x.Id == id);

            return View(xRayCheks);
        }
        catch (Exception ex)
        {
            TempData["ErroMessage"] = $"Error X-Ray Inspection doesn't found: {ex.Message}";
            return View("NotFound");
        }
    }

    //GET: XRayChecks/Create
    public IActionResult Create()
    {
        try
        {
            //  Batches: We get all Production Batches
            ViewData["ProductionBatchId"] = new SelectList(_context.ProductionBatches, "Id", "BatchNumber");

            return View();
        }
        catch (Exception ex)
        {
            ViewData["ErrorMessage"] = $"Error creating X-Ray Inspection: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: XRayChecks/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(XRayCheck xRayCheck)
    {
        // We need to assign the OperatorId based on the currently logged in user,
        // we don't want the user to select it manually for security and data integrity reasons.
        var curretUser = await _userManager.GetUserAsync(User);
        if (curretUser != null)
        {
            xRayCheck.OperatorId = curretUser.Id; // signaciion
        }

        // Quitar validacion manual
        ModelState.Remove("OperatorId"); // Remove the validation for OperatorId since we are assigning it automatically
        ModelState.Remove("Operator"); // Remove the validation for Operator navigation property as well

        try
        {
            if (ModelState.IsValid)
            {
                _context.Add(xRayCheck);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "The X-Ray Inspection was created successfylly!";

                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error creating X-Ray Inspection: {ex.Message}";
            ModelState.AddModelError("", "Unable to save changes. Try againd.");
        }

        // If we got this far, something failed, redisplay form with error messages
        ViewData["ProductionBatchId"] = new SelectList(_context.ProductionBatches, "Id", "BatchNumber");

        return View(xRayCheck);
    }

    // GET XRayChecksController.Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return View("NotFound");
        }

        try
        {
            var xRayCheck = await _context.XRayChecks.FindAsync(id);
            if (xRayCheck is null)
            {
                return View("NotFound");
            }

            ViewData["ProductionBatchId"] = new SelectList(_context.ProductionBatches, "Id", "BatchNumber");

            return View(xRayCheck);
        }
        catch (Exception ex)
        {
            ViewData["ErrorMessage"] = $"Error editing X-Ray Inspection: {ex.Message}";

            return View("NotFound");
        }
    }

    // GET: XRayChecks/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, XRayCheck xRayCheck)
    {
        if (id != xRayCheck.Id)
        {
            return NotFound();
        }

        // We need to assign the OperatorId based on the currently logged in user,
        // we don't want the user to select it manually for security and data integrity reasons.
        var curretUser = await _userManager.GetUserAsync(User);
        if (curretUser != null)
        {
            xRayCheck.OperatorId = curretUser.Id; // signaciion
        }

        // Quitar validacion manual
        ModelState.Remove("OperatorId"); // Remove the validation for OperatorId since we are assigning it automatically
        ModelState.Remove("Operator"); // Remove the validation for Operator navigation property as well

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(xRayCheck);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "The X-Ray Inspection was edited successfylly!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErroeMessage"] = $"Error editing X-Ray Inspection: {ex.Message}";
            }
        }

        ViewData["ProductionBatchId"] = new SelectList(_context.ProductionBatches, "Id", "BatchNumber");

        return View(xRayCheck);
    }

    // GET: XRayChecks/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return View("NotFound");
        }

        try
        {
            var xRayCheck = await _context.XRayChecks
                .Include(x => x.Operator)
                .Include(x => x.ProductionBatch)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (xRayCheck == null)
            {
                return View("NotFound");
            }

            return View(xRayCheck);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error deleting X-Ray Inspection: {ex.Message} ";
            return View("NotFound");
        }
    }

    //POST : XRayChecks/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var xRayCheck = await _context.XRayChecks.FindAsync(id);

            if (xRayCheck != null)
            {
                _context.XRayChecks.Remove(xRayCheck);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Deleting X-Ray Inspection was successfully!";
            }

            RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData[""] = $"ERROR deleting X-Ray Inspection: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }
}