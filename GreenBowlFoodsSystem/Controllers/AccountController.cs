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

    // Constructor: We inject the database context to be able to search for users
    public AccountController(SignInManager<User> signInManager, UserManager<User> userManager)
    {
        this._signInManager = signInManager;
        this._userManager = userManager;
    }

    // GET: /Account/Login
    // This method just shows the Login screen (the HTML form)
    [HttpGet]
    public IActionResult Login()
    {
        if (_signInManager.IsSignedIn(User))
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    // POST: /Account/Login
    // This method receives the data from the form and validates it

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ViewData["ErrorMessage"] = "Please fill all fields.";
            return View();
        }

        // 2. BUSCAR POR EMAIL PRIMERO
        // Como tu sistema usa Email = UserName, buscamos el usuario por su email
        // para asegurarnos de pasar el UserName correcto al SignInManager.
        var user = await _userManager.FindByEmailAsync(email);

        if (user != null)
        {
            // Intentamos hacer login usando el UserName del usuario encontrado (que es igual al email)
            var result = await _signInManager.PasswordSignInAsync(user.UserName!, password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // Si falla
        ViewBag.ErrorMessage = "Invalid Email or Password.";
        return View();
    }

    public async Task<IActionResult> Logout()
    {
        // Delete the identity cookies
        await _signInManager.SignOutAsync();

        // Clear we custon session data
        HttpContext.Session.Clear();

        // Go back to Login
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}