using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GreenBowlFoodsSystem.Controllers;

[Authorize]
public class ShipmentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ShipmentsController(ApplicationDbContext context)
    {
        this._context = context;
    }

    //GET Shipments
    public async Task<IActionResult> Index()
    {
        // Include relationship to show Names (Customera and Finished Products) isntead Of IDs
        var namesOfCustomerAndProducts = await _context.Shipments
            .Include(s => s.Customer)
            .Include(s => s.FinishedProduct)
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        return View(namesOfCustomerAndProducts);
    }

    // GET: Shipments/Create
    public async Task<IActionResult> Create()
    {
        // Load dropdown list for the view
        ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CustomerName");
        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName");

        return View();
    }

    //POST: Shipments/create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Shipment shipment)
    {
        // We ignore validation for these navigate properties bacause the form only sends IDs
        ModelState.Remove("Customer");
        ModelState.Remove("FinishedProduct");
        ModelState.Remove("DeliveryForm");

        if (ModelState.IsValid)
        {
            try
            {
                // Retrieve the product to check Stock and Price
                var productInDb = await _context.FinishedProducts.FindAsync(shipment.FinishedProductId);

                if (productInDb != null)
                {
                    // Validation: If we hace enough stock
                    if (productInDb.QuantityAvailable < shipment.QuantityShipped)
                    {
                        // Error: Insufficient stock
                        TempData["ErrorMessage"] = $"Not enough stock! Only {productInDb.QuantityAvailable} units available.";
                        ModelState.AddModelError("QuantityShipped", "Insufficient inventory.");
                    }
                    else
                    {
                        // Execute Shipment Logic
                        // Deduct Inventory
                        productInDb.QuantityAvailable -= shipment.QuantityShipped;

                        // Calculate financial value (Quantity * Unit Price)
                        //This satisfies the "Sales for a specific period" requirement
                        shipment.TotalValue = shipment.QuantityShipped * productInDb.UnitPrice;

                        // Save Changes
                        _context.Add(shipment); // Register Shipment
                        _context.Update(productInDb);// Update Inventory
                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = "Shipment created successfully! Inventory updated.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Error: Product not found.";
                    ModelState.AddModelError("", "Product not found.");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error processing shipment: {ex.Message}";
            }
        }

        // Reload dropdowns if something failed so the user doesn't lose tyhe form
        ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CustomerName");
        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName");

        return View(shipment);
    }

    // Edit Action: (with Inventory adjustment)
    // GET: Shipments/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var shipment = await _context.Shipments.FindAsync(id);

        if (shipment is null)
        {
            return NotFound();
        }

        ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CustomerName", shipment.CustomerId);
        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName", shipment.FinishedProductId);

        return View(shipment);
    }

    // GET: Shipments/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var shipment = await _context.Shipments
            .Include(s => s.Customer) // brint dtas from the Customers
            .Include(s => s.FinishedProduct) // Brint datas from Products
            .FirstOrDefaultAsync(m => m.Id == id);

        if (shipment is null)
        {
            return NotFound();
        }

        return View(shipment);
    }

    // POST: Shipments/dit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Shipment shipment)
    {
        if (id != shipment.Id) return NotFound();

        // Skip validation for navigation properties
        ModelState.Remove("Customer");
        ModelState.Remove("FinishedProduct");
        ModelState.Remove("DeliveryForm");

        if (ModelState.IsValid)
        {
            try
            {
                //  Get the original shipment from DB (without tracking) to see the OLD quantity
                var originalShipment = await _context.Shipments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);

                //  Get the product to adjust stock
                var productInDb = await _context.FinishedProducts.FindAsync(shipment.FinishedProductId);

                if (originalShipment != null && productInDb != null)
                {
                    // Calculate the Difference
                    // New Qty (80) - Old Qty (50) = +30 (We need to deduct 30 more)
                    // New Qty (20) - Old Qty (50) = -30 (We need to return 30 to stock)
                    int difference = shipment.QuantityShipped - originalShipment.QuantityShipped;

                    // 4. Validate Stock Availability (Only if we are taking MORE)
                    if (difference > 0 && productInDb.QuantityAvailable < difference)
                    {
                        ModelState.AddModelError("QuantityShipped", $"Not enough stock for this increase. Only {productInDb.QuantityAvailable} available.");
                        ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CustomerName", shipment.CustomerId);
                        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName", shipment.FinishedProductId);
                        return View(shipment);
                    }

                    // Apply Inventory Adjustment
                    // Note: If difference is negative (e.g., -30), subtracting a negative adds stock (-(-30) = +30)
                    productInDb.QuantityAvailable -= difference;

                    // Recalculate Financial Value
                    shipment.TotalValue = shipment.QuantityShipped * productInDb.UnitPrice;

                    //  Save Changes
                    _context.Update(shipment);
                    _context.Update(productInDb);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Shipment updated and inventory adjusted!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Shipments.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error to editing shipment: {ex.Message}";
            }
        }

        ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CustomerName", shipment.CustomerId);
        ViewData["FinishedProductId"] = new SelectList(_context.FinishedProducts, "Id", "ProductName", shipment.FinishedProductId);
        return View(shipment);
    }

    // Delete actions (Restory Inventory)
    // GET: Shipments/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var shipment = await _context.Shipments
            .Include(s => s.Customer)
            .Include(s => s.FinishedProduct)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (shipment == null) return NotFound();

        return View(shipment);
    }

    // POST: Shipments/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var shipment = await _context.Shipments.FindAsync(id);

            if (shipment != null)
            {
                // 1. RESTORE INVENTORY 🔄
                // We are canceling the shipment, so the items go back to the warehouse.
                var product = await _context.FinishedProducts.FindAsync(shipment.FinishedProductId);

                if (product != null)
                {
                    product.QuantityAvailable += shipment.QuantityShipped;
                    _context.Update(product);
                }

                // 2. Delete the Record
                _context.Shipments.Remove(shipment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Shipment deleted and stock restored.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErroMessage"] = $"Error to deleting shipment: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}