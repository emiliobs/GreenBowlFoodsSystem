using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenBowlFoodsSystem.Controllers;

[Authorize] // Restricts access to all actions within this controller to authenticated users
public class FinishedProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    // Constructor: Injects the database context for data persistence
    public FinishedProductsController(ApplicationDbContext context)
    {
        this._context = context;
    }

    // GET: FinishedProducts
    // Retrieves the list of finished goods, allowing server-side filtering by Name or SKU
    public async Task<IActionResult> Index(string searchString)
    {
        // Initialize the base query using LINQ
        var products = from p in _context.FinishedProducts
                       select p;

        // Apply filtering logic if a search term is provided by the user
        if (!string.IsNullOrEmpty(searchString))
        {
            // Normalizing search string to lowercase for case-insensitive matching
            products = products.Where(s => s.ProductName.Contains(searchString.ToLower())
                                        || s.SKU.Contains(searchString.ToLower()));
        }

        // Return the ordered list; OrderByDescending ensures newest entries are seen first
        return View(await products.OrderByDescending(p => p.Id).ToListAsync());
    }

    // GET: FinishedProducts/Create
    // Displays the form to register a new product in the catalog
    public IActionResult Create()
    {
        return View();
    }

    // POST: FinishedProducts/Create
    // Processes the creation of a new product with validation and error handling
    [HttpPost]
    [ValidateAntiForgeryToken] // Security: Prevents CSRF attacks
    public async Task<IActionResult> Create(FinishedProduct finishedProduct)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _context.FinishedProducts.Add(finishedProduct);
                await _context.SaveChangesAsync();

                // Success Notification: Stored in TempData to persist across redirects
                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Exception Handling: Logs the error and notifies the user
                TempData["ErrorMessage"] = $"Error creating Finished Product: {ex.Message}";
            }
        }

        return View(finishedProduct);
    }

    // GET: FinishedProducts/Edit/5
    // Retrieves the product details to be modified
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return View("NotFound");
        }

        var finnishedProduct = await _context.FinishedProducts.FindAsync(id);
        if (finnishedProduct is null)
        {
            return View("NotFound");
        }

        return View(finnishedProduct);
    }

    // POST: FinishedProducts/Edit/5
    // Updates existing product data in the database
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, FinishedProduct finishedProduct)
    {
        if (id != finishedProduct.Id)
        {
            return View("NotFound");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(finishedProduct);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Product update successfully!";
            }
            catch (Exception ex)
            {
                // UI Feedback: Triggers the error alert mechanism in the Layout
                ViewData["ErroMessage"] = $"Error updating Finished Product: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        return View(finishedProduct);
    }

    // GET: FinishedProducts/Details/5
    // Displays specific metadata for a single product entry
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return View("NotFound");
        }

        var finishedProduct = await _context.FinishedProducts.FirstOrDefaultAsync(fp => fp.Id == id);

        if (finishedProduct is null)
        {
            return NotFound();
        }

        return View(finishedProduct);
    }

    // GET: FinishedProducts/Delete/5
    // Restricted Action: Only users with the "Admin" role can access the delete confirmation
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return View("NotFound");
        }

        var finishedProduct = await _context.FinishedProducts.FirstOrDefaultAsync(fp => fp.Id == id);
        if (finishedProduct is null)
        {
            return View("NotFound");
        }

        return View(finishedProduct);
    }

    // POST: FinishedProducts/Delete/5
    // Confirms and executes the permanent removal of a product from the database
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var finishedProduct = await _context.FinishedProducts.FindAsync(id);
            if (finishedProduct is not null)
            {
                _context.FinishedProducts.Remove(finishedProduct);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Product deleted from catalog!";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error deleting finished Product: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}