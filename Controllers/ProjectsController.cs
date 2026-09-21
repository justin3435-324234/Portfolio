using Microsoft.AspNetCore.Mvc;
using Portfolio.Data;

namespace Portfolio.Controllers;

public class ProjectsController : Controller
{
    // Table of Contents: grouped by category
    public IActionResult Index(string? category)
    {
        var projects = PortfolioStore.Projects.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(category))
            projects = projects.Where(p => p.Category == category);
        ViewData["Categories"] = PortfolioStore.Projects.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
        ViewData["ActiveCategory"] = category ?? "All";
        return View(projects.OrderByDescending(p => p.Year).ToList());
    }

    // Detail page + comment section
    public IActionResult Details(int id)
    {
        var project = PortfolioStore.GetProject(id);
        if (project == null) return NotFound();
        ViewData["Comments"] = PortfolioStore.GetComments(id);
        ViewData["Related"] = PortfolioStore.Projects.Where(p => p.Id != id && p.Category == project.Category).Take(3).ToList();
        return View(project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Comment(int id, string author, string text)
    {
        var project = PortfolioStore.GetProject(id);
        if (project == null) return NotFound();
        if (string.IsNullOrWhiteSpace(author) || string.IsNullOrWhiteSpace(text))
        {
            TempData["Toast"] = "Please add your name and a comment.";
            return RedirectToAction(nameof(Details), new { id });
        }
        PortfolioStore.AddComment(new Models.Comment
        {
            ProjectId = id,
            Author = author.Trim()[..Math.Min(60, author.Trim().Length)],
            Text = text.Trim()[..Math.Min(1000, text.Trim().Length)]
        });
        TempData["Toast"] = "Comment posted. Thanks!";
        return RedirectToAction(nameof(Details), new { id });
    }
}
