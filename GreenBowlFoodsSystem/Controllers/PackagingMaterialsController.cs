using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;

namespace GreenBowlFoodsSystem.Controllers
{
    public class PackagingMaterialsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PackagingMaterialsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PackagingMaterials
        public async Task<IActionResult> Index()
        {
            return View(await _context.PackagingMaterials.Include(p => p.Supplier).ToListAsync());
        }

        // GET: PackagingMaterials/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }

            var packagingMaterial = await _context.PackagingMaterials
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (packagingMaterial == null)
            {
                return View("NotFound");
            }

            return View(packagingMaterial);
        }

        // GET: PackagingMaterials/Create
        public IActionResult Create()
        {
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName");
            return View();
        }

        // POST: PackagingMaterials/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PackagingMaterial packagingMaterial)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(packagingMaterial);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Packaging material created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating packaging material: {ex.Message}";
            }

            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName", packagingMaterial.SupplierId);
            return View(packagingMaterial);
        }

        // GET: PackagingMaterials/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }

            var packagingMaterial = await _context.PackagingMaterials.FindAsync(id);
            if (packagingMaterial == null)
            {
                return View("NotFound");
            }

            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName", packagingMaterial.SupplierId);
            return View(packagingMaterial);
        }

        // POST: PackagingMaterials/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PackagingMaterial packagingMaterial)
        {
            if (id != packagingMaterial.Id)
            {
                return View("NotFound");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(packagingMaterial);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Packaging material updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating packaging material: {ex.Message}";
                }
            }

            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "SupplierName", packagingMaterial.SupplierId);
            return View(packagingMaterial);
        }

        // GET: PackagingMaterials/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }

            var packagingMaterial = await _context.PackagingMaterials
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (packagingMaterial == null)
            {
                return View("NotFound");
            }

            return View(packagingMaterial);
        }

        // POST: PackagingMaterials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var packagingMaterial = await _context.PackagingMaterials.FindAsync(id);
                if (packagingMaterial != null)
                {
                    _context.PackagingMaterials.Remove(packagingMaterial);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Packaging material deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting packaging material: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PackagingMaterialExists(int id)
        {
            return _context.PackagingMaterials.Any(e => e.Id == id);
        }
    }
}