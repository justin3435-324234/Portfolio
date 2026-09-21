using Microsoft.AspNetCore.Mvc;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Controllers;

public class AdminController : Controller
{
    private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "true";

    private IActionResult RequireAdmin()
    {
        if (!IsAdmin) return RedirectToAction("Login", "Account");
        return null!;
    }

    public IActionResult Index()
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;
        ViewData["TotalProjects"] = PortfolioStore.Projects.Count;
        ViewData["TotalComments"] = PortfolioStore.Comments.Count;
        ViewData["Categories"] = PortfolioStore.Projects.Select(p => p.Category).Distinct().Count();
        ViewData["Featured"] = PortfolioStore.Projects.Count(p => p.Featured);
        return View(PortfolioStore.Projects.OrderBy(p => p.Id).ToList());
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;
        var p = PortfolioStore.GetProject(id);
        if (p == null) return NotFound();
        return View(p);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;
        return View("Edit", new Project { Year = DateTime.Now.Year, Status = "Completed", Category = "Web Development" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Project model, string? techCsv, string? featuresCsv, IFormFile? photoFile)
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;
        model.Technologies = (techCsv ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        model.Features = (featuresCsv ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        if (string.IsNullOrWhiteSpace(model.Title))
        {
            ViewData["Error"] = "Title is required.";
            return View(model);
        }
        if (photoFile != null && photoFile.Length > 0)
        {
            var upload = await SaveUpload(photoFile);
            if (upload == null)
            {
                ViewData["Error"] = "Invalid image. Use JPG, PNG, GIF or WebP under 5MB.";
                return View(model);
            }
            model.ImageUrl = upload;
        }
        PortfolioStore.UpsertProject(model);
        TempData["Toast"] = "Project saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;
        PortfolioStore.DeleteProject(id);
        TempData["Toast"] = "Project deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reset()
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;
        PortfolioStore.ResetToDefault();
        TempData["Toast"] = "Reset to default demo content.";
        return RedirectToAction(nameof(Index));
    }

    // Projects tab (full CRUD list — same data, dedicated tab)
    public IActionResult Projects()
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;
        return View(PortfolioStore.Projects.OrderBy(p => p.Id).ToList());
    }

    // Appearance tab — overall UI colors
    [HttpGet]
    public IActionResult Appearance()
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;
        return View(PortfolioStore.Site);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Appearance(SiteSettings model, string? preset)
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;
        var site = PortfolioStore.Site;
        if (!string.IsNullOrWhiteSpace(preset))
        {
            ApplyPreset(site, preset);
        }
        else
        {
            site.Primary = model.Primary; site.Secondary = model.Secondary; site.Accent = model.Accent;
            site.Background = model.Background; site.Surface = model.Surface;
        }
        PortfolioStore.UpdateSite(site);
        TempData["Toast"] = "Theme updated.";
        return RedirectToAction(nameof(Appearance));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ResetTheme()
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;
        var site = PortfolioStore.Site;
        var fresh = new SiteSettings();
        site.Primary = fresh.Primary; site.Secondary = fresh.Secondary; site.Accent = fresh.Accent;
        site.Background = fresh.Background; site.Surface = fresh.Surface;
        PortfolioStore.UpdateSite(site);
        TempData["Toast"] = "Theme reset.";
        return RedirectToAction(nameof(Appearance));
    }

    // Profile tab — name, bio, links
    [HttpGet]
    public IActionResult Profile()
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;
        return View(PortfolioStore.Site);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(SiteSettings model, IFormFile? photoFile, bool removePhoto = false, string? skillsCsv = null)
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;
        var site = PortfolioStore.Site;
        site.DisplayName = model.DisplayName; site.BrandSuffix = model.BrandSuffix;
        site.Headline = model.Headline; site.Intro = model.Intro; site.Bio = model.Bio;
        site.Location = model.Location; site.Availability = model.Availability;
        site.GitHubUrl = model.GitHubUrl; site.FacebookUrl = model.FacebookUrl; site.LinkedInUrl = model.LinkedInUrl;
        if (skillsCsv != null)
            site.Skills = skillsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        if (!string.IsNullOrWhiteSpace(model.ProfileImageUrl))
            site.ProfileImageUrl = model.ProfileImageUrl;
        if (removePhoto)
            site.ProfileImageUrl = "";
        if (photoFile != null && photoFile.Length > 0)
        {
            var upload = await SaveUpload(photoFile);
            if (upload == null)
            {
                ViewData["Error"] = "Invalid image. Use JPG, PNG, GIF or WebP under 5MB.";
                return View(site);
            }
            site.ProfileImageUrl = upload;
        }
        PortfolioStore.UpdateSite(site);
        TempData["Toast"] = "Profile updated.";
        return RedirectToAction(nameof(Profile));
    }

    private static readonly string[] AllowedExt = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    private async Task<string?> SaveUpload(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExt.Contains(ext) || file.Length > 5 * 1024 * 1024) return null;
        var webRoot = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootPath;
        var dir = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(dir);
        var name = Guid.NewGuid().ToString("N") + ext;
        await using var stream = System.IO.File.Create(Path.Combine(dir, name));
        await file.CopyToAsync(stream);
        return "/uploads/" + name;
    }

    private static void ApplyPreset(SiteSettings site, string preset)
    {
        switch (preset.ToLowerInvariant())
        {
            case "neon": site.Primary = "#8b5cf6"; site.Secondary = "#3b82f6"; site.Accent = "#22d3ee"; site.Background = "#07070a"; site.Surface = "#12121a"; break;
            case "aurora": site.Primary = "#22d3ee"; site.Secondary = "#3b82f6"; site.Accent = "#4ade80"; site.Background = "#041014"; site.Surface = "#0b1e24"; break;
            case "synthwave": site.Primary = "#ec4899"; site.Secondary = "#8b5cf6"; site.Accent = "#3b82f6"; site.Background = "#0d0714"; site.Surface = "#1a0f24"; break;
            case "ocean": site.Primary = "#3b82f6"; site.Secondary = "#22d3ee"; site.Accent = "#67e8f9"; site.Background = "#050b14"; site.Surface = "#0e1a2b"; break;
            case "crimson": site.Primary = "#ef4444"; site.Secondary = "#8b5cf6"; site.Accent = "#f59e0b"; site.Background = "#100607"; site.Surface = "#1c0d10"; break;
            case "mono": site.Primary = "#e5e5e5"; site.Secondary = "#a3a3a3"; site.Accent = "#ffffff"; site.Background = "#000000"; site.Surface = "#111111"; break;
        }
    }
}
