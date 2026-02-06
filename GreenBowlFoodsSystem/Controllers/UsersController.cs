using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GreenBowlFoodsSystem.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public UsersController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        this._userManager = userManager;
    }

    // GET: Users
    public async Task<IActionResult> Index()
    {
        return View(await _context.Users.ToListAsync());
    }

    // GET: Users/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // GET: Users/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Users/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User user)
    {
        try
        {
            // 1. AUTO-ASSIGN USERNAME
            // We force the UserName to be the same as the Email.
            // This ensures login is always done via Email.
            user.UserName = user.Email;

            // Validate Password manually if needed
            if (string.IsNullOrEmpty(user.Password))
            {
                ModelState.AddModelError("Password", "Password is required.");
            }

            if (ModelState.IsValid)
            {
                // Create the user using UserManager
                // NOTE: This method AUTOMATICALLY saves changes to the database.
                // We changed variable name 'userNameExist' to 'result' because it's an IdentityResult object.
                var result = await _userManager.CreateAsync(user, user.Password);

                if (result.Succeeded)
                {
                    // 3. Assign Role (Admin or Staff)
                    if (!string.IsNullOrEmpty(user.Role))
                    {
                        await _userManager.AddToRoleAsync(user, user.Role);
                    }

                    TempData["SuccessMessage"] = "New user registered successfully!";
                    return RedirectToAction(nameof(Index));
                }

                // Handle Identity Errors (e.g., "Password too weak", "Email taken")
                // If creation failed, we show WHY to the user.
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error registering user: {ex.Message}";
        }

        // If we got here, something failed, show the form again with errors
        return View(user);
    }

    // GET: Users/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // Use UserManager to find the user by ID (Safe way)
        var user = await _userManager.FindByIdAsync(id.ToString()!);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    // POST: Users/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, User user)
    {
        if (id != user.Id)
        {
            return NotFound();
        }

        // Note: We don't validate Password here because we don't update it in this form.
        // We clear the ModelState error for Password to allow saving changes.
        ModelState.Remove("Password");

        // We validate the form data before processing
        if (ModelState.IsValid)
        {
            try
            {
                // Find the REAL user in the database first
                var userInDb = await _userManager.FindByIdAsync(id.ToString());

                if (userInDb == null)
                {
                    return NotFound();
                }

                // Update Personal Info
                userInDb.FirstName = user.FirstName;
                userInDb.LastName = user.LastName;
                userInDb.PhoneNumber = user.PhoneNumber;
                userInDb.Role = user.Role;

                // Sync Email and UserName
                userInDb.Email = user.Email;
                userInDb.UserName = user.Email; // Keep them synced!

                // Save changes using Identity Manager
                var result = await _userManager.UpdateAsync(userInDb);

                if (result.Succeeded)
                {
                    // Update Role logic (Remove old, add new) if needed...
                    // (Simpler version for now):
                    var currentRoles = await _userManager.GetRolesAsync(userInDb);
                    await _userManager.RemoveFromRolesAsync(userInDb, currentRoles);
                    if (!string.IsNullOrEmpty(user.Role))
                    {
                        await _userManager.AddToRoleAsync(userInDb, user.Role);
                    }

                    TempData["SuccessMessage"] = "User details updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                // Handle Identity Errors (Database level validation)
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating user: {ex.Message}";
            }
        }

        // If ModelState is NOT valid (e.g. empty fields), code jumps here directly
        return View(user);
    }

    // GET: Users/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // Find user to display details before deleting
        var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // POST: Users/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            // 1. Find the user using UserManager
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user != null)
            {
                // 2. Delete using Identity (Safe Delete)
                // This removes roles, claims, and logins associated with the user automatically
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "User removed from the system.";
                }
                else
                {
                    // If Identity refuses to delete, show why
                    foreach (var error in result.Errors)
                    {
                        TempData["ErrorMessage"] += $" {error.Description}";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Cannot delete user. It may have related records: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}