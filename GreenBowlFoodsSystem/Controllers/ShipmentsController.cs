using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace GreenBowlFoodsSystem.Controllers;

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
            .Include(s => s.FinishedProduct).ToListAsync();

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
                        TempData["ErrorMessasge"] = $"Not enough stock! Only {productInDb.QuantityAvailable} units available.";
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

                        TempData[""] = "Shipment creates successfully! Inventory updated.";
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
}