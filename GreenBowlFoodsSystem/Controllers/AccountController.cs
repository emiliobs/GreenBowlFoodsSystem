using GreenBowlFoodsSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenBowlFoodsSystem.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    // Constructor: We inject the database context to be able to search for users
    public AccountController(ApplicationDbContext context)
    {
        this._context = context;
    }

    // GET: /Account/Login
    // This method just shows the Login screen (the HTML form)
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // POST: /Account/Login
    // This method receives the data from the form and validates it

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        // Search for a user with the matching Username and Password
        // Note: In a real production app, we should compare hashed passwords, not plain text.
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

        if (user is not null)
        {
            // We store the username so we can check if the user is logged in later
            HttpContext.Session.SetString("UserSession", user.Username);
            HttpContext.Session.SetString("UserRole", user.Role);

            // User found! Valid credentials.
            // For now, we just redirect to the Home page or Dashboard.
            // Later, we will add the logic to "remember" the user (Session/Cookies).
            return RedirectToAction("Index", "Home");
        }

        // User not found or wrong password.
        // We add an error message to display in the View.
        ViewBag.ErrorMessage = "Invalid username or password.";
        return View();
    }

    public IActionResult Logout()
    {
        // Clear the session
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}