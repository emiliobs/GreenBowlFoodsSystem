using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenBowlFoodsSystem.Controllers
{
    public class FinishedProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FinishedProductsController(ApplicationDbContext context)
        {
            this._context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.FinishedProducts.ToListAsync());
        }

        // GET: FinishedProducts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FinishedProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FinishedProduct finishedProduct)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.FinishedProducts.Add(finishedProduct);
                    await _context.SaveChangesAsync();

                    // Success Alert
                    TempData["SuccessMessage"] = "Product created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error creating Finished Product: {ex.Message}";
                }
            }

            return View(finishedProduct);
        }

        // GET: FinishedProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var finnishedProduct = await _context.FinishedProducts.FindAsync(id);
            if (finnishedProduct is null)
            {
                return NotFound();
            }

            return View(finnishedProduct);
        }

        // POST: FinishedProducts?Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, FinishedProduct finishedProduct)
        {
            if (id != finishedProduct.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(finishedProduct);
                    await _context.SaveChangesAsync();

                    // Success Alert
                    TempData["SuccessMessage"] = "Product update successfully!";
                }
                catch (Exception ex)
                {
                    // Error Message: Triggers the red SweetAlert in _Layout.cshtml
                    ViewData["ErroMessage"] = $"Error updating Finished Product: {ex.Message}";
                }

                return RedirectToAction(nameof(Index));
            }

            return View(finishedProduct);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var finishedProduct = await _context.FinishedProducts.FirstOrDefaultAsync(fp => fp.Id == id);

            if (finishedProduct is null)
            {
                return NotFound();
            }

            return View(finishedProduct);
        }

        // GET: FinishedProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var finishedProduct = await _context.FinishedProducts.FirstOrDefaultAsync(fp => fp.Id == id);
            if (finishedProduct is null)
            {
                return NotFound();
            }

            return View(finishedProduct);
        }

        // POST: FinishedProducts/Delete/5
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

                    // Success Alert
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
}