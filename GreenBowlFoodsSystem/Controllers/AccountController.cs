using GreenBowlFoodsSystem.Data;
using GreenBowlFoodsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace GreenBowlFoodsSystem.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    // Constructor: Dependency Injection of Identity managers to handle user sessions and data
    public AccountController(SignInManager<User> signInManager, UserManager<User> userManager)
    {
        this._signInManager = signInManager;
        this._userManager = userManager;
    }

    // GET: /Account/Login
    // Displays the login view. Redirects to Home if the user is already authenticated.
    [HttpGet]
    public IActionResult Login()
    {
        if (_signInManager.IsSignedIn(User))
        {
            // Prevents authenticated users from accessing the login page again
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    // POST: /Account/Login
    // Validates credentials and initiates the secure session
    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        // Basic validation to ensure both fields are provided before processing
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ViewData["ErrorMessage"] = "Please fill all fields.";
            return View();
        }

        // SEARCH BY EMAIL FIRST
        // Since the system uses Email as the primary identifier, we fetch the user entity by email.
        // This ensures the correct UserName is passed to the SignInManager.
        var user = await _userManager.FindByEmailAsync(email);

        if (user != null)
        {
            // Attempt to sign in using the retrieved UserName and provided password
            // isPersistent: false (session cookie only), lockoutOnFailure: false (standard login)
            var result = await _signInManager.PasswordSignInAsync(user.UserName!, password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Successful authentication redirects to the main dashboard
                return RedirectToAction("Index", "Home");
            }
        }

        // If authentication fails, notify the user with a generic message for security
        ViewBag.ErrorMessage = "Invalid Email or Password.";
        return View();
    }

    // Sign out method to terminate the user session
    public async Task<IActionResult> Logout()
    {
        // Delete the identity cookies from the browser
        await _signInManager.SignOutAsync();

        // Clear all custom session data stored in the server
        HttpContext.Session.Clear();

        // Redirect the user back to the login screen
        return RedirectToAction("Login");
    }

    // Handles redirection when a user attempts to access a restricted module without permissions
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}