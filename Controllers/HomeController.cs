using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var projects = PortfolioStore.Projects;
        ViewData["Featured"] = projects.Where(p => p.Featured).Take(2).ToList();
        ViewData["All"] = projects;
        return View(projects);
    }

    public IActionResult About() => View();

    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
