using System.Text.Json;
using Portfolio.Models;

namespace Portfolio.Data;

// JSON-file backed store (no database, survives restart, editable from Admin).
// Thread-safe via lock. Seeded with demo content on first run.
public static class PortfolioStore
{
    private static readonly object _lock = new();
    private static string _dataDir = "";
    private static string ProjectsFile => Path.Combine(_dataDir, "projects.json");
    private static string CommentsFile => Path.Combine(_dataDir, "comments.json");
    private static string SiteFile => Path.Combine(_dataDir, "site.json");

    public static List<Project> Projects { get; private set; } = new();
    public static List<Comment> Comments { get; private set; } = new();
    public static SiteSettings Site { get; private set; } = new();

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    public static void Init(string contentRoot)
    {
        _dataDir = Path.Combine(contentRoot, "Data");
        Directory.CreateDirectory(_dataDir);
        lock (_lock)
        {
            Projects = LoadOrSeedProjects();
            Comments = LoadComments();
            Site = LoadOrSeedSite();
        }
    }

    // ---- Projects ----
    public static Project? GetProject(int id)
    {
        lock (_lock) return Projects.FirstOrDefault(p => p.Id == id);
    }

    public static void UpsertProject(Project p)
    {
        lock (_lock)
        {
            if (p.Id == 0)
            {
                p.Id = Projects.Count == 0 ? 1 : Projects.Max(x => x.Id) + 1;
                if (string.IsNullOrWhiteSpace(p.Slug))
                    p.Slug = Slugify(p.Title);
                Projects.Add(p);
            }
            else
            {
                var ex = Projects.FirstOrDefault(x => x.Id == p.Id);
                if (ex == null) Projects.Add(p);
                else
                {
                    ex.Title = p.Title; ex.Slug = string.IsNullOrWhiteSpace(p.Slug) ? Slugify(p.Title) : p.Slug;
                    ex.ShortDescription = p.ShortDescription; ex.Description = p.Description;
                    ex.Category = p.Category; ex.Status = p.Status; ex.Year = p.Year;
                    ex.Featured = p.Featured; ex.ImageUrl = p.ImageUrl;
                    ex.Technologies = p.Technologies; ex.LiveUrl = p.LiveUrl; ex.GitHubUrl = p.GitHubUrl;
                    ex.Problem = p.Problem; ex.Solution = p.Solution; ex.Features = p.Features;
                }
            }
            SaveProjects();
        }
    }

    public static void DeleteProject(int id)
    {
        lock (_lock)
        {
            Projects.RemoveAll(p => p.Id == id);
            Comments.RemoveAll(c => c.ProjectId == id);
            SaveProjects(); SaveComments();
        }
    }

    public static void ResetToDefault()
    {
        lock (_lock)
        {
            Projects = DefaultProjects();
            Comments = new List<Comment>();
            Site = new SiteSettings();
            SaveProjects(); SaveComments(); SaveSite();
        }
    }

    // ---- Site (theme + profile) ----
    public static void UpdateSite(SiteSettings s)
    {
        lock (_lock)
        {
            Site = s;
            SaveSite();
        }
    }

    public static void ResetSite()
    {
        lock (_lock)
        {
            Site = new SiteSettings();
            SaveSite();
        }
    }

    // ---- Comments ----
    public static List<Comment> GetComments(int projectId)
    {
        lock (_lock) return Comments.Where(c => c.ProjectId == projectId).OrderByDescending(c => c.CreatedAt).ToList();
    }

    public static void AddComment(Comment c)
    {
        lock (_lock)
        {
            c.Id = Comments.Count == 0 ? 1 : Comments.Max(x => x.Id) + 1;
            c.CreatedAt = DateTime.UtcNow;
            Comments.Add(c);
            SaveComments();
        }
    }

    // ---- Persistence ----
    private static List<Project> LoadOrSeedProjects()
    {
        try
        {
            if (File.Exists(ProjectsFile))
            {
                var json = File.ReadAllText(ProjectsFile);
                var list = JsonSerializer.Deserialize<List<Project>>(json, JsonOpts);
                if (list != null && list.Count > 0) return list;
            }
        }
        catch { /* fall through to seed */ }
        var seed = DefaultProjects();
        try { File.WriteAllText(ProjectsFile, JsonSerializer.Serialize(seed, JsonOpts)); } catch { }
        return seed;
    }

    private static List<Comment> LoadComments()
    {
        try
        {
            if (File.Exists(CommentsFile))
            {
                var json = File.ReadAllText(CommentsFile);
                var list = JsonSerializer.Deserialize<List<Comment>>(json, JsonOpts);
                if (list != null) return list;
            }
        }
        catch { }
        return new List<Comment>();
    }

    private static void SaveProjects()
    {
        try { File.WriteAllText(ProjectsFile, JsonSerializer.Serialize(Projects, JsonOpts)); } catch { }
    }

    private static void SaveComments()
    {
        try { File.WriteAllText(CommentsFile, JsonSerializer.Serialize(Comments, JsonOpts)); } catch { }
    }

    private static SiteSettings LoadOrSeedSite()
    {
        try
        {
            if (File.Exists(SiteFile))
            {
                var json = File.ReadAllText(SiteFile);
                var s = JsonSerializer.Deserialize<SiteSettings>(json, JsonOpts);
                if (s != null) return s;
            }
        }
        catch { }
        var seed = new SiteSettings();
        try { File.WriteAllText(SiteFile, JsonSerializer.Serialize(seed, JsonOpts)); } catch { }
        return seed;
    }

    private static void SaveSite()
    {
        try { File.WriteAllText(SiteFile, JsonSerializer.Serialize(Site, JsonOpts)); } catch { }
    }

    private static string Slugify(string s)
    {
        var slug = new string(s.ToLowerInvariant().Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-').ToArray());
        return slug.Trim().Replace(' ', '-');
    }

    public static List<Project> DefaultProjects() => new()
    {
        new Project { Id=1, Title="Neon Task Manager", Slug="neon-task-manager",
            ShortDescription="Kanban task app with drag-and-drop and dark neon UI.",
            Description="A full-featured Kanban task manager with boards, labels, and local persistence. Built to stay fast with hundreds of cards.",
            Category="Web Development", Status="Completed", Year=2026, Featured=true,
            ImageUrl="https://images.unsplash.com/photo-1555066931-4365d14bab8c?w=1200&q=80",
            Technologies=new(){ "C#", "ASP.NET Core", "JavaScript", "SQL" },
            LiveUrl="", GitHubUrl="https://github.com/",
            Problem="Teams needed a fast board without heavy SaaS overhead.",
            Solution="Built a lightweight MVC + JS board with JSON persistence and instant filtering.",
            Features=new(){ "Drag-and-drop boards", "Labels & filters", "Progress stats", "Responsive layout" } },
        new Project { Id=2, Title="Aurora Weather App", Slug="aurora-weather",
            ShortDescription="Beautiful weather dashboard with hourly forecasts.",
            Description="Weather dashboard consuming a public API with geolocation, hourly/daily views and animated backgrounds.",
            Category="Application Development", Status="Completed", Year=2025, Featured=true,
            ImageUrl="https://images.unsplash.com/photo-1504608524841-42fe6f032b4b?w=1200&q=80",
            Technologies=new(){ "JavaScript", "HTML", "CSS", "REST API" },
            LiveUrl="", GitHubUrl="https://github.com/",
            Problem="Default weather apps felt cluttered and slow.",
            Solution="Designed a minimal aurora-themed UI with cached API responses.",
            Features=new(){ "Geolocation", "Hourly forecast", "Animated sky", "PWA-ready" } },
        new Project { Id=3, Title="Campus E-Commerce", Slug="campus-ecommerce",
            ShortDescription="Storefront for student sellers with cart and checkout flow.",
            Description="Complete storefront: catalog, cart, checkout simulation, order history and admin product management.",
            Category="Web Development", Status="In Progress", Year=2026, Featured=false,
            ImageUrl="https://images.unsplash.com/photo-1557821552-171051766652?w=1200&q=80",
            Technologies=new(){ "C#", "ASP.NET Core", "Bootstrap", "SQL" },
            LiveUrl="", GitHubUrl="https://github.com/",
            Problem="Students had no simple place to sell on campus.",
            Solution="Built an MVC storefront with Table of Contents categories and detail pages.",
            Features=new(){ "Catalog + TOC", "Cart flow", "Order tracking", "Admin CRUD" } },
        new Project { Id=4, Title="Dev Portfolio API", Slug="dev-portfolio-api",
            ShortDescription="REST API serving projects, skills and blog posts.",
            Description="Clean REST API with versioning, pagination and Swagger docs, backing this portfolio.",
            Category="Cloud Development", Status="Completed", Year=2025, Featured=false,
            ImageUrl="https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=1200&q=80",
            Technologies=new(){ "C#", ".NET", "REST", "AWS" },
            LiveUrl="", GitHubUrl="https://github.com/",
            Problem="Portfolio data was hardcoded in views.",
            Solution="Extracted a versioned API with seed data and caching.",
            Features=new(){ "Versioning", "Pagination", "Swagger", "Caching" } },
        new Project { Id=5, Title="Pixel Chat Prototype", Slug="pixel-chat",
            ShortDescription="Realtime chat prototype with rooms and typing indicators.",
            Description="Realtime prototype with rooms, presence and typing indicators. Edit GitHub link in Admin mode.",
            Category="Prototyping", Status="Completed", Year=2024, Featured=false,
            ImageUrl="https://images.unsplash.com/photo-1611606063065-ee7946f0787a?w=1200&q=80",
            Technologies=new(){ "Node.js", "JavaScript", "WebSocket" },
            LiveUrl="", GitHubUrl="https://github.com/",
            Problem="Needed a quick realtime demo for class.",
            Solution="Prototyped with WebSockets, then documented to port to .NET SignalR.",
            Features=new(){ "Rooms", "Typing indicator", "Presence", "History" } },
        new Project { Id=6, Title="UI Component Kit", Slug="ui-component-kit",
            ShortDescription="Reusable glassmorphism component library.",
            Description="A small design system: buttons, cards, modals, toasts and timeline, used across this site.",
            Category="UI/UX Design", Status="Completed", Year=2024, Featured=false,
            ImageUrl="https://images.unsplash.com/photo-1561070791-2526d30994b5?w=1200&q=80",
            Technologies=new(){ "HTML", "CSS", "JavaScript" },
            LiveUrl="", GitHubUrl="https://github.com/",
            Problem="Every school project reinvented the same styles.",
            Solution="Built one kit with tokens, then reused it here.",
            Features=new(){ "Design tokens", "12 components", "Dark mode", "Reduced-motion support" } },
    };
}
