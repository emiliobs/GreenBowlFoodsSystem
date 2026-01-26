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
    public class RawMaterialsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RawMaterialsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RawMaterials
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.RawMaterials.Include(r => r.Supplier);
            return View(await applicationDbContext.ToListAsync());
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
                return NotFound();
            }

            return View(rawMaterial);
        }

        // GET: RawMaterials/Create
        public IActionResult Create()
        {
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Email");
            return View();
        }

        // POST: RawMaterials/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MaterialName,LotNumber,QuantityInStock,Unit,ExpiryDate,SupplierId")] RawMaterial rawMaterial)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rawMaterial);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Email", rawMaterial.SupplierId);
            return View(rawMaterial);
        }

        // GET: RawMaterials/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rawMaterial = await _context.RawMaterials.FindAsync(id);
            if (rawMaterial == null)
            {
                return NotFound();
            }
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Email", rawMaterial.SupplierId);
            return View(rawMaterial);
        }

        // POST: RawMaterials/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MaterialName,LotNumber,QuantityInStock,Unit,ExpiryDate,SupplierId")] RawMaterial rawMaterial)
        {
            if (id != rawMaterial.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rawMaterial);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RawMaterialExists(rawMaterial.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Email", rawMaterial.SupplierId);
            return View(rawMaterial);
        }

        // GET: RawMaterials/Delete/5
        public async Task<IActionResult> Delete(int? id)
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
                return NotFound();
            }

            return View(rawMaterial);
        }

        // POST: RawMaterials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rawMaterial = await _context.RawMaterials.FindAsync(id);
            if (rawMaterial != null)
            {
                _context.RawMaterials.Remove(rawMaterial);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RawMaterialExists(int id)
        {
            return _context.RawMaterials.Any(e => e.Id == id);
        }
    }
}
