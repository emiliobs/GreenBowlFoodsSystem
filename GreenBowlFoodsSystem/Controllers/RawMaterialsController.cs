using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GreenBowlFoodsSystem.Controllers;

[Authorize]
public class RawMaterialsController : Controller
{
    // Dependency Injection: Access the database context
    private readonly ApplicationDbContext _context;

    public RawMaterialsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Display the main List (Inventory)
    // GET: RawMaterials
    public async Task<IActionResult> Index()
    {
        //
        var applicationDbContext = _context.RawMaterials.Include(r => r.Supplier);
        return View(await applicationDbContext.ToListAsync());
    }

    // GET: RawMaterials/Create
    // Renders the form to add a new materia
    public IActionResult Create()
    {
        // Prepare the Dropdown List for suppliers,
        ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName");
        return View();
    }

    // POST: RawMaterials/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // Receives the form data and saves the new material to the database.
    [HttpPost]
    [ValidateAntiForgeryToken] // Security measure to prevent CSRF attacks
    public async Task<IActionResult> Create([Bind("Id,MaterialName,LotNumber,QuantityInStock,Unit,ExpiryDate,SupplierId")] RawMaterial rawMaterial)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(rawMaterial);
                await _context.SaveChangesAsync();

                // Succces Message: Triggers the green SweetAlert in _Layout.cshtml

                TempData["SuccessMessage"] = "Materia added to inventory!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Error Message: Triggers the red SweetAlert in _Layout.cshtml
                TempData[""] = $"Error adding material: {ex.Message}";
            }
        }

        // If validation fails, reload the Supplier Dropdown so it's not empty
        ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName", rawMaterial.SupplierId);

        return View(rawMaterial);
    }

    // GET: RawMaterials/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return View("NotFound");
        }

        var rawMaterial = await _context.RawMaterials.FindAsync(id);
        if (rawMaterial == null)
        {
            return NotFound();
        }

        // Reload the suppliar Dropdown, selecting the current suplier
        ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName", rawMaterial.SupplierId);

        return View(rawMaterial);
    }

    // POST: RawMaterials/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,MaterialName,LotNumber,QuantityInStock,Unit,ExpiryDate,SupplierId")] RawMaterial rawMaterial)
    {
        if (id != rawMaterial.Id)
        {
            return View("NotFound");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(rawMaterial);
                await _context.SaveChangesAsync();

                // Succces Message: Triggers the green SweetAlert in _Layout.cshtml
                TempData["SuccessMessage"] = "Material details updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RawMaterialExists(rawMaterial.Id))
                {
                    return View("NotFound");
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                // Error Message: Triggers the red SweetAlert in _Layout.cshtml
                ViewData["ErroMessage"] = $"Error updating material: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName", rawMaterial.SupplierId);

        return View(rawMaterial);
    }

    // GET: RawMaterials/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var rawMaterial = await _context.RawMaterials
            .Include(r => r.Supplier)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (rawMaterial == null)
        {
            return View("NotFound");
        }

        return View(rawMaterial);
    }

    // GET: RawMaterials/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return View("NotFound");
        }

        var rawMaterial = await _context.RawMaterials
            .Include(r => r.Supplier) // Include Supplier to show name in confirmation screen
            .FirstOrDefaultAsync(m => m.Id == id);

        if (rawMaterial == null)
        {
            return View("NotFound");
        }

        return View(rawMaterial);
    }

    // POST: RawMaterials/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var rawMaterial = await _context.RawMaterials.FindAsync(id);
            if (rawMaterial != null)
            {
                _context.RawMaterials.Remove(rawMaterial);
            }

            await _context.SaveChangesAsync();

            // Succces Message: Triggers the green SweetAlert in _Layout.cshtml
            TempData["SuccessMessage"] = "Material deleted from inventory!";
        }
        catch (Exception ex)
        {
            // Error Message: Triggers the red SweetAlert in _Layout.cshtml
            TempData["ErrorMessage"] = $"Error deleting material: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool RawMaterialExists(int id)
    {
        return _context.RawMaterials.Any(e => e.Id == id);
    }
}