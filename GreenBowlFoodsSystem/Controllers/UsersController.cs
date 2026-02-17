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

[Authorize(Roles = "Admin")] // Global Security: Only users with the 'Admin' role can access these actions
public class UsersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    // Constructor: Dependency Injection of the Database Context and Identity's UserManager
    public UsersController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        this._userManager = userManager;
    }

    // GET: Users
    // Lists all registered users with server-side searching capabilities
    public async Task<IActionResult> Index(string searchString)
    {
        // Persistence: Maintain the current search term in the search box
        ViewData["CurrentFilter"] = searchString;

        // Initialize user query as IQueryable for efficient server-side filtering
        var users = _context.Users.AsQueryable();

        // Search Logic: Filter by First Name, Last Name, Email, or Role
        if (!string.IsNullOrEmpty(searchString))
        {
            users = users.Where(u => u.FirstName.Contains(searchString.ToLower())
                                  || u.LastName.Contains(searchString.ToLower())
                                  || u.Email!.Contains(searchString.ToLower())
                                  || u.Role.Contains(searchString.ToLower()));
        }

        // Sorting: Order the list alphabetically by Last Name and execute query
        return View(await users.OrderByDescending(u => u.LastName).ToListAsync());
    }

    // GET: Users/Details/5
    // Retrieves metadata for a specific user record
    public async Task<IActionResult> Details(int? id)
    {
        // Safety: If no ID is provided, return custom error view
        if (id == null)
        {
            return View("NotFound");
        }

        // Database Lookup: Find the user by primary key
        var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
        if (user == null)
        {
            return View("NotFound");
        }

        return View(user);
    }

    // GET: Users/Create
    // Renders the registration form for new system operators
    public IActionResult Create()
    {
        return View();
    }

    // POST: Users/Create
    // Processes new user registration, password hashing, and role assignment
    [HttpPost]
    [ValidateAntiForgeryToken] // Security: Guard against CSRF attacks
    public async Task<IActionResult> Create(User user)
    {
        try
        {
            // AUTO-ASSIGN USERNAME
            // We force the UserName to be the same as the Email, This ensures login is always done via Email.
            user.UserName = user.Email;

            // Integrity Check: Find if the email is already registered to another account
            var existingUser = await _userManager.FindByEmailAsync(user.Email!);
            if (existingUser != null)
            {
                // UI Feedback: Add error specifically to the email field
                ModelState.AddModelError("Email", "This email address is already in use by another user.");
                TempData["ErrorMessage"] = "This email address is already in use by another user.";
            }

            // Manual Validation: Ensure password field is not empty before sending to Identity
            if (string.IsNullOrEmpty(user.Password))
            {
                ModelState.AddModelError("Password", "Password is required.");
                TempData["ErrorMessage"] = "Password is required.";
            }

            // Proceed only if all validation rules are met
            if (ModelState.IsValid)
            {
                // CREATE THE USER USING USERMANAGER
                // This method hashes the password and saves the user to the database automatically.
                var result = await _userManager.CreateAsync(user, user.Password);

                if (result.Succeeded)
                {
                    // ROLE ASSIGNMENT: Link the user to 'Admin' or 'Staff' groups
                    if (!string.IsNullOrEmpty(user.Role))
                    {
                        await _userManager.AddToRoleAsync(user, user.Role);
                    }

                    // Success Feedback: Trigger SweetAlert notification
                    TempData["SuccessMessage"] = "New user registered successfully!";
                    return RedirectToAction(nameof(Index));
                }

                // SECURITY LOGIC: Handle Identity errors (e.g., weak password, illegal characters)
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }
        catch (Exception ex)
        {
            // Exception Management: Capture technical failures
            TempData["ErrorMessage"] = $"Error registering user: {ex.Message}";
        }

        // Fallback: If creation fails, redisplay the form with existing data and errors
        return View(user);
    }

    // GET: Users/Edit/5
    // Prepares the edit interface using Identity's secure find method
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return View("NotFound");
        }

        // FETCHING DATA: Use UserManager to find the user by ID (Thread-safe and optimized)
        var user = await _userManager.FindByIdAsync(id.ToString()!);
        if (user == null)
        {
            return View("NotFound");
        }
        return View(user);
    }

    // POST: Users/Edit/5
    // Updates user profile information and synchronizes role changes
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, User user)
    {
        if (id != user.Id)
        {
            return View("NotFound");
        }

        // UI LOGIC: We clear Password validation because this form is for profile updates only.
        ModelState.Remove("Password");

        // Validate the incoming form data
        if (ModelState.IsValid)
        {
            // UNIQUENESS CHECK: Ensure the new email doesn't belong to another user
            var userWithSameEamil = await _userManager.FindByEmailAsync(user.Email!);

            // If we find someone with the same email, and the ID is different from current user
            if (userWithSameEamil != null && userWithSameEamil.Id != user.Id)
            {
                ModelState.AddModelError("Error", "This email is already taken bu another user.");
                TempData["ErrorMessage"] = "This email address is already in use by another user.";

                return View(user);
            }

            try
            {
                // DATA SYNC: Find the original user entity in the database
                var userInDb = await _userManager.FindByIdAsync(id.ToString());

                if (userInDb == null)
                {
                    return View("NotFound");
                }

                // Update Profile Properties
                userInDb.FirstName = user.FirstName;
                userInDb.LastName = user.LastName;
                userInDb.PhoneNumber = user.PhoneNumber;
                userInDb.Role = user.Role;

                // Sync Email and UserName to maintain consistency in login credentials
                userInDb.Email = user.Email;
                userInDb.UserName = user.Email;

                // Execute Update: Save profile changes using Identity Manager
                var result = await _userManager.UpdateAsync(userInDb);

                if (result.Succeeded)
                {
                    // ROLE SYNC: Remove old roles and assign the newly selected role
                    var currentRoles = await _userManager.GetRolesAsync(userInDb);
                    await _userManager.RemoveFromRolesAsync(userInDb, currentRoles);
                    if (!string.IsNullOrEmpty(user.Role))
                    {
                        await _userManager.AddToRoleAsync(userInDb, user.Role);
                    }

                    TempData["SuccessMessage"] = "User details updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                // Error Aggregation: Capture Identity Result errors
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

        return View(user);
    }

    // GET: Users/Delete/5
    // Shows confirmation view. Restricted by [Authorize(Roles = "Admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return View("NotFound");
        }

        // Fetch user metadata to identify the account before deletion
        var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
        if (user == null)
        {
            return View("NotFound");
        }

        return View(user);
    }

    // POST: Users/Delete/5
    // Executes secure account removal via UserManager
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            // 1. DATA ACCESS: Retrieve the target user entity
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user != null)
            {
                // 2. SECURE DELETE: Identity manages role and claim removal automatically
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "User removed from the system.";
                }
                else
                {
                    // Failure Reporting: Capture reasons why Identity refused to delete
                    foreach (var error in result.Errors)
                    {
                        TempData["ErrorMessage"] += $" {error.Description}";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Catching database constraint errors (e.g., user is linked to production records)
            TempData["ErrorMessage"] = $"Cannot delete user. It may have related records: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    // Helper: Internal check to verify account existence via the EF context
    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}