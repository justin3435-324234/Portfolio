# Modern Portfolio — Prefinals Quiz (ASP.NET Core MVC)

## Login details (hardcoded demo)
- Username: `admin`
- Password: `password123`
- Login at: `/Account/Login` (or "Admin Login" in navbar)
- After login you land on `/Admin` dashboard.

> Demo local auth only — NOT production secure.
> For production, move authentication and content management to a secure backend.

## Run
```powershell
cd Portfolio
dotnet run
```
Open the URL shown (e.g. http://localhost:5xxx).

## Quiz requirements covered
- [x] MVC application presenting projects clearly
- [x] GitHub project links + short description + thumbnail per project
- [x] Hardcoded login (above) documented here
- [x] Table of contents: `/Projects` grouped by category with filter
- [x] Detail page per project: `/Projects/Details/{id}` with image, description, problem/solution, features, tech, links
- [x] Comment section per project (name + text, persisted to `Data/comments.json`)

## Admin mode (edit GitHub links without code)
1. Login as admin.
2. Dashboard lists all projects with stats.
3. `Edit` any project → paste your real **GitHub URL**, Live URL, thumbnail Image URL → Save.
4. `+ Add Project` / `Delete` / `Reset to default` included.
5. Data persists in `Portfolio/Data/projects.json` + `comments.json` (no database).

## Structure
```
Controllers/HomeController, ProjectsController, AccountController, AdminController
Models/Project, Comment, LoginViewModel
Data/PortfolioStore.cs (JSON-file store + seed)
Views/Home, Projects (Index=TOC, Details+comments), Account/Login, Admin (Index, Edit)
wwwroot/css/site.css, js/site.js (particles, reveal, magnetic buttons, toasts)
```
