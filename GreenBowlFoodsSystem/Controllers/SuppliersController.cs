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
public class SuppliersController : Controller
{
    private readonly ApplicationDbContext _context;

    public SuppliersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Suppliers
    // Displays the directory of approved raw material suppliers.
    public async Task<IActionResult> Index(string searchString)
    {
        // Save the current search filter to ViewData so it remains in the input field
        ViewData["CurrentFilter"] = searchString;

        // 1. Initialize query
        var suppliers = _context.Suppliers.AsQueryable();

        // 2. Apply Search Filter if user entered text
        if (!string.IsNullOrEmpty(searchString))
        {
            // Search by Supplier Name, Contact Person, or Email Address
            suppliers = suppliers.Where(s => s.SupplierName!.Contains(searchString.ToLower())
                                          || s.ContactPerson!.Contains(searchString.ToLower())
                                          || s.Email!.Contains(searchString.ToLower()));
        }

        // 3. Execute query: Order by Supplier Name (Alphabetical)
        
        return View(await suppliers.OrderBy(s => s.SupplierName).ToListAsync());
    }

    // GET: Suppliers/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return View("NotFound");
        }

        var supplier = await _context.Suppliers.FirstOrDefaultAsync(m => m.Id == id);

        if (supplier == null)
        {
            return View("NotFound");
        }

        return View(supplier);
    }

    // GET: Suppliers/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Suppliers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Supplier supplier)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(supplier);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "New supplier registered successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error registering supplier: {ex.Message}";
            }
        }

        return View(supplier);
    }

    // GET: Suppliers/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return View("NotFound");
        }

        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null)
        {
            return View("NotFound");
        }

        return View(supplier);
    }

    // POST: Suppliers/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Supplier supplier)
    {
        if (id != supplier.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(supplier);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Supplier details updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SupplierExists(supplier.Id))
                {
                    TempData["ErrorMessage"] = "Supplier not fount, it might have been deleted.";

                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not update supplier. Try again: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
        return View(supplier);
    }

    // GET: Suppliers/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var supplier = await _context.Suppliers.FirstOrDefaultAsync(m => m.Id == id);
        if (supplier == null)
        {
            return NotFound();
        }

        return View(supplier);
    }

    // POST: Suppliers/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier != null)
            {
                _context.Suppliers.Remove(supplier);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Supplier removed from the system.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessahe"] = $"Cannot delete supplier. It may have related records: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool SupplierExists(int id)
    {
        return _context.Suppliers.Any(e => e.Id == id);
    }
}