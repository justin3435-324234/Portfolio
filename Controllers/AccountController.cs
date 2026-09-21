using Microsoft.AspNetCore.Mvc;
using Portfolio.Models;

namespace Portfolio.Controllers;

// Hardcoded demo login (frontend-template style, NOT production secure).
// For production deployment, move authentication and content management to a secure backend.
public class AccountController : Controller
{
    public const string DemoUsername = "admin";
    public const string DemoPassword = "password123";

    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("IsAdmin") == "true")
            return RedirectToAction("Index", "Admin");
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model)
    {
        if (model.Username?.Trim() == DemoUsername && model.Password == DemoPassword)
        {
            HttpContext.Session.SetString("IsAdmin", "true");
            TempData["Toast"] = "Welcome back, Admin.";
            return RedirectToAction("Index", "Admin");
        }
        ViewData["Error"] = "Invalid username or password. Hint: see README.md";
        return View(model);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Remove("IsAdmin");
        TempData["Toast"] = "Logged out.";
        return RedirectToAction("Index", "Home");
    }
}
