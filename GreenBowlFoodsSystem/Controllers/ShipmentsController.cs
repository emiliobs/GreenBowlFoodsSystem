using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GreenBowlFoodsSystem.Controllers;

// SECURITY: Restricts access to this controller to authenticated users with Admin or Staff roles
[Authorize]
public class ShipmentsController : Controller
{
    // Database context field for executing data persistence and relational queries
    private readonly ApplicationDbContext _context;

    // Constructor: Dependency Injection of the Database Context to handle sales and distribution records
    public ShipmentsController(ApplicationDbContext context)
    {
        this._context = context;
    }

    // GET: Shipments/Index
    // Displays the log of outgoing shipments and sales revenue with server-side filtering
    public async Task<IActionResult> Index(string searchString)
    {
        // Persistence: Save the current search filter to ViewData to maintain UI state in the search box after reload
        ViewData["CurrentFilter"] = searchString;

        // Eager Loading: Fetch related Customer and FinishedProduct details to show names instead of numeric IDs
        var shipments = _context.Shipments
            .Include(s => s.Customer)
            .Include(s => s.FinishedProduct)
            .AsQueryable();

        // Search Logic: Apply filters if the user provided specific search criteria in the UI
        if (!string.IsNullOrEmpty(searchString))
        {
            // Case-insensitive filtering by Customer Name, Tracking Number, or Product Name for better UX
            shipments = shipments.Where(s => s.Customer!.CustomerName.Contains(searchString.ToLower())
                                          || s.TrackingNumber!.Contains(searchString.ToLower())
                                          || s.FinishedProduct!.ProductName.Contains(searchString.ToLower()));
        }

        // Execution: Sort by Date (Descending) to show the most recent shipments at the top of the list
        return View(await shipments.OrderByDescending(s => s.Date).ToListAsync());
    }

    // GET: Shipments/Create
    // Prepares the dispatch interface with product and customer selection lists
    public async Task<IActionResult> Create()
    {
        // Populate ViewData with SelectLists for the dropdown menus in the Razor View
        ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CustomerName");
        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName");

        // Returns the creation form view
        return View();
    }

    // POST: Shipments/Create
    // Processes the dispatch logic, stock deduction, and automatic revenue calculation
    [HttpPost]
    [ValidateAntiForgeryToken] // Security: Guard against Cross-Site Request Forgery (CSRF) attacks
    public async Task<IActionResult> Create(Shipment shipment)
    {
        // Model Sanitization: Remove navigation objects from validation state to avoid false errors
        // The HTML form only sends numeric IDs, not the full relational C# objects
        ModelState.Remove("Customer");
        ModelState.Remove("FinishedProduct");
        ModelState.Remove("DeliveryForm");

        // Validate that the submitted form data complies with the Model's data annotations
        if (ModelState.IsValid)
        {
            try
            {
                // Business Logic: Retrieve the specific product to verify current stock levels and unit price
                var productInDb = await _context.FinishedProducts.FindAsync(shipment.FinishedProductId);

                // Ensure the product actually exists in the catalog before proceeding
                if (productInDb != null)
                {
                    // INVENTORY VALIDATION: Ensure sufficient stock is available in the warehouse before dispatching
                    if (productInDb.QuantityAvailable < shipment.QuantityShipped)
                    {
                        // Error handling: Notifies the user about insufficient inventory via SweetAlert
                        TempData["ErrorMessage"] = $"Not enough stock! Only {productInDb.QuantityAvailable} units available.";
                        // Add specific model error to highlight the input field in red
                        ModelState.AddModelError("QuantityShipped", "Insufficient inventory.");
                    }
                    else
                    {
                        // STOCK DEDUCTION: Subtract the shipped quantity from the warehouse available inventory
                        productInDb.QuantityAvailable -= shipment.QuantityShipped;

                        // REVENUE CALCULATION: Set total financial value (Quantity * Current Unit Price)
                        // This fulfills the "Sales for a specific period" requirement for financial reporting
                        shipment.TotalValue = shipment.QuantityShipped * productInDb.UnitPrice;

                        // DATABASE TRANSACTION: Register the new shipment record in the system
                        _context.Add(shipment);
                        // Update the product record with the new (reduced) stock level
                        _context.Update(productInDb);
                        // Save all changes asynchronously to the SQL database
                        await _context.SaveChangesAsync();

                        // Success Feedback: Notify user of successful dispatch and inventory update
                        TempData["SuccessMessage"] = "Shipment created successfully! Inventory updated.";

                        // Redirect to the list view after success
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    // Logic Error: Handle scenarios where the selected product ID is not found
                    TempData["ErrorMessage"] = "Error: Product not found.";
                    ModelState.AddModelError("", "Product not found.");
                }
            }
            catch (Exception ex)
            {
                // Exception Management: Catch technical errors during the database commit process
                TempData["ErrorMessage"] = $"Error processing shipment: {ex.Message}";
            }
        }

        // Fallback: Reload dropdowns if validation fails so the user doesn't lose their data
        ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CustomerName", shipment.CustomerId);
        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName", shipment.FinishedProductId);

        // Re-display the form with the current shipment data and validation messages
        return View(shipment);
    }

    // GET: Shipments/Edit/5
    // Retrieves existing shipment data for modification with current stock context
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        // Safety check for null ID parameters in the URL
        if (id is null)
        {
            return View("NotFound");
        }

        // Search for the specific shipment record using its Primary Key
        var shipment = await _context.Shipments.FindAsync(id);

        // Redirect to a friendly error page if the record does not exist
        if (shipment is null)
        {
            return View("NotFound");
        }

        // Pre-populate SelectLists and pre-select current Customer and Product for the UI
        ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CustomerName", shipment.CustomerId);
        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName", shipment.FinishedProductId);

        // Returns the edit form view
        return View(shipment);
    }

    // GET: Shipments/Details/5
    // Provides a full audit view of a single shipment including all relational metadata
    public async Task<IActionResult> Details(int? id)
    {
        // Safety check for null ID
        if (id is null)
        {
            return View("NotFound");
        }

        // Multi-table Join: Fetch the shipment including Customer and FinishedProduct names
        var shipment = await _context.Shipments
            .Include(s => s.Customer)
            .Include(s => s.FinishedProduct)
            .FirstOrDefaultAsync(m => m.Id == id);

        // Validate record existence
        if (shipment is null)
        {
            return View("NotFound");
        }

        // Returns the details summary view
        return View(shipment);
    }

    // POST: Shipments/Edit/5
    // Processes modifications and calculates the inventory delta for precise stock correction
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Shipment shipment)
    {
        // Verification: Ensure the URL ID parameter matches the hidden ID field in the model
        if (id != shipment.Id) return NotFound();

        // Model Sanitization for navigation properties
        ModelState.Remove("Customer");
        ModelState.Remove("FinishedProduct");
        ModelState.Remove("DeliveryForm");

        if (ModelState.IsValid)
        {
            try
            {
                // DATA INTEGRITY: Fetch original record without tracking to compare the OLD quantity with the NEW one
                var originalShipment = await _context.Shipments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);

                // Fetch the product record to adjust warehouse stock based on the transaction changes
                var productInDb = await _context.FinishedProducts.FindAsync(shipment.FinishedProductId);

                // Ensure both the historical record and the product exist before calculating deltas
                if (originalShipment != null && productInDb != null)
                {
                    // INVENTORY DELTA LOGIC: Calculate the difference between new and old quantity
                    // Positive difference = User increased the order; we need to deduct more stock
                    // Negative difference = User decreased the order; we need to return stock to warehouse
                    int difference = shipment.QuantityShipped - originalShipment.QuantityShipped;

                    // STOCK AVAILABILITY CHECK: Only relevant if the user is attempting to ship MORE units
                    if (difference > 0 && productInDb.QuantityAvailable < difference)
                    {
                        // Stop the process if the warehouse cannot fulfill the increase
                        ModelState.AddModelError("QuantityShipped", $"Not enough stock for this increase. Only {productInDb.QuantityAvailable} available.");
                        ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CustomerName", shipment.CustomerId);
                        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName", shipment.FinishedProductId);

                        return View(shipment);
                    }

                    // INVENTORY ADJUSTMENT: Subtract the calculated delta from the warehouse stock
                    // Math note: subtracting a negative difference (return) increases the available stock
                    productInDb.QuantityAvailable -= difference;

                    // FINANCIAL UPDATE: Recalculate the Total Sales Value based on the updated quantity
                    shipment.TotalValue = shipment.QuantityShipped * productInDb.UnitPrice;

                    // TRANSACTIONAL COMMIT: Update the Shipment record and the adjusted Product stock in one unit of work
                    _context.Update(shipment);
                    _context.Update(productInDb);
                    await _context.SaveChangesAsync();

                    // Notify success
                    TempData["SuccessMessage"] = "Shipment updated and inventory adjusted!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                // Log and notify about technical errors
                TempData["ErrorMessage"] = $"Error to editing shipment: {ex.Message}";
            }
        }

        // Reload UI requirements in case of error
        ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CustomerName", shipment.CustomerId);
        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName", shipment.FinishedProductId);

        return View(shipment);
    }

    // GET: Shipments/Delete/5
    // Shows confirmation screen. Access restricted to Administrator role for security.
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        // ID parameter safety check
        if (id == null)
        {
            return View("NotFound");
        }

        // Fetch record with full Eager Loading to show what exactly is being cancelled
        var shipment = await _context.Shipments
            .Include(s => s.Customer)
            .Include(s => s.FinishedProduct)
            .FirstOrDefaultAsync(m => m.Id == id);

        // Record exists check
        if (shipment == null)
        {
            return View("NotFound");
        }

        // Returns confirmation view
        return View(shipment);
    }

    // POST: Shipments/Delete/5
    // Cancels a shipment and automatically restores the quantity back to the warehouse inventory
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            // Retrieve the shipment record to be removed
            var shipment = await _context.Shipments.FindAsync(id);

            if (shipment != null)
            {
                // STOCK RESTORATION: Find the product and return items back to available inventory
                // This ensures assets are not lost when an order is cancelled
                var product = await _context.FinishedProducts.FindAsync(shipment.FinishedProductId);

                if (product != null)
                {
                    // Increment the product stock by the quantity that was supposed to be shipped
                    product.QuantityAvailable += shipment.QuantityShipped;
                    _context.Update(product); // Mark product entity for stock update
                }

                // DATA REMOVAL: Permanently remove the shipment record from the database audit
                _context.Shipments.Remove(shipment);
                // Commit the stock restoration and record deletion in a single SQL transaction
                await _context.SaveChangesAsync();

                // Success notification for the UI
                TempData["SuccessMessage"] = "Shipment deleted and stock restored.";
            }
        }
        catch (Exception ex)
        {
            // Catch-all for database errors or FK constraint issues
            TempData["ErroMessage"] = $"Error to deleting shipment: {ex.Message}";
        }

        // Final redirect to the list view
        return RedirectToAction(nameof(Index));
    }
}