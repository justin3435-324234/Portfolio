namespace Portfolio.Models;

public class Project
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string ShortDescription { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "Web Development";
    public string Status { get; set; } = "Completed";
    public int Year { get; set; } = 2026;
    public bool Featured { get; set; }
    public string ImageUrl { get; set; } = "";
    public List<string> Technologies { get; set; } = new();
    public string LiveUrl { get; set; } = "";
    public string GitHubUrl { get; set; } = "";
    public string Problem { get; set; } = "";
    public string Solution { get; set; } = "";
    public List<string> Features { get; set; } = new();
}

public class Comment
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Author { get; set; } = "";
    public string Text { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class LoginViewModel
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class SiteSettings
{
    // Theme
    public string Primary { get; set; } = "#8b5cf6";
    public string Secondary { get; set; } = "#3b82f6";
    public string Accent { get; set; } = "#22d3ee";
    public string Background { get; set; } = "#07070a";
    public string Surface { get; set; } = "#12121a";
    // Profile
    public string DisplayName { get; set; } = "JUSTIN";
    public string BrandSuffix { get; set; } = ".DEV";
    public string Headline { get; set; } = "CRAFTING DIGITAL EXPERIENCES";
    public string Intro { get; set; } = "Hi, I'm Justin — a developer focused on modern web apps, clean MVC architecture, and delightful UI. Browse my work, open any project, and leave a comment.";
    public string Bio { get; set; } = "I build modern digital experiences with C#, ASP.NET Core MVC, and clean frontend design. This portfolio is my Prefinals Quiz submission: an MVC app with a table of contents, detail pages, comments, and a hardcoded admin login.";
    public string Location { get; set; } = "Philippines";
    public string Availability { get; set; } = "Available for projects";
    public string ProfileImageUrl { get; set; } = "";
    public string GitHubUrl { get; set; } = "https://github.com/";
    public string FacebookUrl { get; set; } = "https://facebook.com/";
    public string LinkedInUrl { get; set; } = "https://linkedin.com/";
    public List<string> Skills { get; set; } = new() { "C#", "ASP.NET Core", "SQL", "JavaScript", "HTML", "CSS", "Git", "UI/UX" };
}
