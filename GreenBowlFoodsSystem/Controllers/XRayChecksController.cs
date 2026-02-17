using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenBowlFoodsSystem.Controllers;

[Authorize] // Restricts access to authenticated users to ensure quality records integrity
public class XRayChecksController : Controller
{
    // Database context field for executing data persistence and relational queries
    private readonly ApplicationDbContext _context;

    // Identity UserManager field to handle and verify current user session data
    private readonly UserManager<User> _userManager;

    // Constructor: Injects the ApplicationDbContext and UserManager via Dependency Injection for modularity
    public XRayChecksController(ApplicationDbContext context, UserManager<User> userManager)
    {
        this._context = context;
        this._userManager = userManager;
    }

    // GET: XRayChecks (List all inspections)
    // Retrieves a complete history of safety checks performed on production batches
    public async Task<IActionResult> Index()
    {
        try
        {
            // Eager Loading Implementation: Fetch the Operator and ProductionBatch entities in a single SQL query
            // This prevents the N+1 problem and allows the UI to display names and batch numbers instead of raw IDs
            var getOperatorAndproductioBatch = await _context.XRayChecks
                .Include(x => x.Operator) // Join with Users table to identify the technician
                .Include(x => x.ProductionBatch) // Join with Production table to identify the specific lot
                .ToListAsync(); // Execute the query asynchronously to prevent thread blocking

            // Return the fully populated list of inspections to the Index view
            return View(getOperatorAndproductioBatch);
        }
        catch (Exception ex)
        {
            // Error Handling: Store the technical exception message in ViewData for the UI error alert system
            ViewData["ErrorMessage"] = $"Error loading index: {ex.Message}";

            // Return an empty list to prevent the Razor view from crashing during null iteration
            return View(new List<XRayCheck>());
        }
    }

    // GET: XRayChecks/Details/5
    // Fetches a single inspection record with its corresponding relational metadata for auditing
    public async Task<IActionResult> Details(int? id)
    {
        // Safety check for null ID parameters in the request URL to avoid server exceptions
        if (id is null)
        {
            return View("NotFound");
        }

        try
        {
            // Database Query: Load the specific check record including the Operator and Production Batch info
            var xRayCheks = await _context.XRayChecks
                .Include(x => x.Operator) // Eager load the user responsible for the check
                .Include(x => x.ProductionBatch) // Eager load the batch details for context
                .FirstOrDefaultAsync(x => x.Id == id); // Locate the unique record by ID

            // Pass the retrieved entity to the details view
            return View(xRayCheks);
        }
        catch (Exception ex)
        {
            // Temporary error notification passed to the next request via TempData
            TempData["ErroMessage"] = $"Error X-Ray Inspection doesn't found: {ex.Message}";

            // Redirect to a generic error page if the record is inaccessible
            return View("NotFound");
        }
    }

    // GET: XRayChecks/Create
    // Prepares the user interface for a new food safety quality inspection entry
    public IActionResult Create()
    {
        try
        {
            // UI Selection Logic: Fetch all production batches to populate the dropdown selection menu
            // Values are IDs and displayed text are the Batch Numbers
            ViewData["ProductionBatchId"] = new SelectList(_context.ProductionBatches, "Id", "BatchNumber");

            // Return the empty creation form to the operator
            return View();
        }
        catch (Exception ex)
        {
            // Redirect to index with error feedback if the dropdown data fails to load from the DB
            ViewData["ErrorMessage"] = $"Error creating X-Ray Inspection: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: XRayChecks/Create
    // Validates the quality data and persists the inspection record, securing the operator's identity
    [HttpPost]
    [ValidateAntiForgeryToken] // Security measure: Protects against Cross-Site Request Forgery (CSRF) attacks
    public async Task<IActionResult> Create(XRayCheck xRayCheck)
    {
        // Audit Trail Logic: Automatically retrieve and assign the current logged-in user's ID
        // This ensures the operator identity is tamper-proof and strictly linked to the active session
        var curretUser = await _userManager.GetUserAsync(User);
        if (curretUser != null)
        {
            // Automatic assignment to satisfy data integrity requirements
            xRayCheck.OperatorId = curretUser.Id;
        }

        // Model Sanitization: Manually remove the Operator properties from the validation state
        // Since these are assigned in the backend code, they should not be validated from form input
        ModelState.Remove("OperatorId");
        ModelState.Remove("Operator");

        try
        {
            // Check if the submitted form data meets all Model data annotation requirements
            if (ModelState.IsValid)
            {
                // Track the new quality record in the database context
                _context.Add(xRayCheck);

                // Commit the changes to the SQL database asynchronously
                await _context.SaveChangesAsync();

                // Success Feedback: Store a message for the SweetAlert notification system in the next view
                TempData["SuccessMessage"] = "The X-Ray Inspection was created successfylly!";

                // Redirect to the list view after a successful save operation
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            // Inform the user about the failure and add an error to the model for visual UI feedback
            TempData["ErrorMessage"] = $"Error creating X-Ray Inspection: {ex.Message}";
            ModelState.AddModelError("", "Unable to save changes. Try againd.");
        }

        // Fallback Logic: Re-populate the batch dropdown if validation fails to return to the form safely
        ViewData["ProductionBatchId"] = new SelectList(_context.ProductionBatches, "Id", "BatchNumber");

        // Re-display the view with the current data and validation error messages
        return View(xRayCheck);
    }

    // GET: XRayChecks/Edit/5
    // Retrieves existing inspection data to allow the operator to correct data entry errors
    public async Task<IActionResult> Edit(int? id)
    {
        // Safety check for null ID parameter in the URL
        if (id is null)
        {
            return View("NotFound");
        }

        try
        {
            // Locate the specific inspection record in the database tracker
            var xRayCheck = await _context.XRayChecks.FindAsync(id);

            // Redirect to NotFound if the ID does not exist in the database
            if (xRayCheck is null)
            {
                return View("NotFound");
            }

            // Reload the production batch list for the dropdown menu
            ViewData["ProductionBatchId"] = new SelectList(_context.ProductionBatches, "Id", "BatchNumber");

            // Pass the existing record to the edit form
            return View(xRayCheck);
        }
        catch (Exception ex)
        {
            // General error feedback for database retrieval issues
            ViewData["ErrorMessage"] = $"Error editing X-Ray Inspection: {ex.Message}";

            return View("NotFound");
        }
    }

    // POST: XRayChecks/Edit/5
    // Processes modifications to a quality record and updates the database
    [HttpPost]
    [ValidateAntiForgeryToken] // Security measure: Verifies the request origin
    public async Task<IActionResult> Edit(int id, XRayCheck xRayCheck)
    {
        // Route protection: Ensure the ID in the URL parameter matches the ID in the posted data
        if (id != xRayCheck.Id)
        {
            return NotFound();
        }

        // Session Sync: Re-verify and assign the current operator performing the record modification
        var curretUser = await _userManager.GetUserAsync(User);
        if (curretUser != null)
        {
            // Ensure the operator responsible for the change is recorded
            xRayCheck.OperatorId = curretUser.Id;
        }

        // Model State maintenance: Remove auto-assigned properties from the validation engine
        ModelState.Remove("OperatorId");
        ModelState.Remove("Operator");

        // Validate the updated data against model rules
        if (ModelState.IsValid)
        {
            try
            {
                // Mark the entity as Modified for Entity Framework to track the change
                _context.Update(xRayCheck);

                // Execute the SQL UPDATE command asynchronously
                await _context.SaveChangesAsync();

                // Notification: Trigger success SweetAlert message
                TempData["SuccessMessage"] = "The X-Ray Inspection was edited successfylly!";

                // Return to the main list after a successful update
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Catch database or logic errors and notify the UI
                TempData["ErroeMessage"] = $"Error editing X-Ray Inspection: {ex.Message}";
            }
        }

        // Re-populate the selection dropdown if the model state validation failed
        ViewData["ProductionBatchId"] = new SelectList(_context.ProductionBatches, "Id", "BatchNumber");

        // Return the user to the edit form to correct validation errors
        return View(xRayCheck);
    }

    // GET: XRayChecks/Delete/5
    // Renders the deletion confirmation view. Access restricted to Administrator role for audit safety.
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        // Safety check for null ID
        if (id is null)
        {
            return View("NotFound");
        }

        try
        {
            // Deep Loading Logic: Fetch all details to identify the record for the Admin before final deletion
            var xRayCheck = await _context.XRayChecks
                .Include(x => x.Operator) // Identify who performed the check originally
                .Include(x => x.ProductionBatch) // Identify which batch is affected
                .FirstOrDefaultAsync(x => x.Id == id);

            // Validation: Ensure the record is still present in the database
            if (xRayCheck == null)
            {
                return View("NotFound");
            }

            // Return the record to the confirmation view
            return View(xRayCheck);
        }
        catch (Exception ex)
        {
            // General error feedback for database access issues
            TempData["ErrorMessage"] = $"Error deleting X-Ray Inspection: {ex.Message} ";
            return View("NotFound");
        }
    }

    // POST: XRayChecks/Delete/5
    // Finalizes the permanent removal of the quality inspection record from the database audit
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken] // Security: Ensures the delete request is legitimate
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            // Retrieve the target record from the database tracker
            var xRayCheck = await _context.XRayChecks.FindAsync(id);

            // If the record is found, remove it from the tracker and commit
            if (xRayCheck != null)
            {
                // Mark the entity for permanent removal
                _context.XRayChecks.Remove(xRayCheck);

                // Execute the SQL DELETE command asynchronously
                await _context.SaveChangesAsync();

                // Success Feedback: Notify the Admin via SweetAlert
                TempData["SuccessMessage"] = $"Deleting X-Ray Inspection was successfully!";
            }

            // Redirect back to the Index list
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            // Inform about database constraints or connection failures preventing the delete
            TempData[""] = $"ERROR deleting X-Ray Inspection: {ex.Message}";
        }

        // Final fallback redirection to the Index
        return RedirectToAction(nameof(Index));
    }
}