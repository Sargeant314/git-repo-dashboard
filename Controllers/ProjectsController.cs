using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepoDashboard.Models;
using RepoDashboard.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace RepoDashboard.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectNote>>> GetProjects()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Only fetch projects owned by the logged-in user
            return await _context.Projects.Where(p => p.UserId == userId).ToListAsync();
        }

        /*
        * Fetch all public repositories for a username from Github
        * GET: github/{username}
        */
        [HttpGet("github/{username}")]
        public async Task<ActionResult<IEnumerable<RepoDashboard.Services.GitHubRepoDto>>> GetGitHubRepos(
            string username,
            [FromServices] RepoDashboard.Services.GitHubService gitHubService)
        {
            // Read-only external API call; no DB filtering needed here
            var repos = await gitHubService.GetUserReposAsync(username);
            return Ok(repos);
        }

        // POST: api/projects
        [HttpPost]
        public async Task<ActionResult<ProjectNote>> PostProject(ProjectNote project)
        {
            // Assign the new project to the logged-in user
            project.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProjects), new { id = project.Id }, project);
        }

        // POST: api/projects/import-github/{username}
        [HttpPost("import-github/{username}")]
        public async Task<IActionResult> BulkImportGitHubRepos(
            string username, 
            [FromServices] RepoDashboard.Services.GitHubService gitHubService)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var repos = await gitHubService.GetUserReposAsync(username);
            int addedCount = 0;

            foreach (var repo in repos)
            {
                // Check if THIS user already has THIS repo
                bool exists = await _context.Projects.AnyAsync(p => p.UserId == userId && p.RepoName.ToLower() == repo.Name.ToLower());
                if (!exists)
                {
                    _context.Projects.Add(new ProjectNote
                    {
                        UserId = userId, // Assign to user
                        RepoName = repo.Name,
                        PrivateNotes = repo.Description ?? "Imported from GitHub.",
                        PriorityLevel = "Medium",
                        Status = "Backlog",
                        Language = repo.Language ?? "Unknown" 
                    });
                    addedCount++;
                }
            }

            if (addedCount > 0) await _context.SaveChangesAsync();
            return Ok(new { importedCount = addedCount });
        }

        // POST: api/projects/import-github/{username}/{repoName}
        [HttpPost("import-github/{username}/{repoName}")]
        public async Task<IActionResult> ImportSingleGitHubRepo(
            string username, 
            string repoName,
            [FromServices] RepoDashboard.Services.GitHubService gitHubService)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var repo = await gitHubService.GetSingleRepoAsync(username, repoName);
            
            if (repo == null) return NotFound(new { message = "Repository not found on GitHub." });

            // Check if THIS user already has THIS repo
            bool exists = await _context.Projects.AnyAsync(p => p.UserId == userId && p.RepoName.ToLower() == repo.Name.ToLower());
            if (exists) return Conflict(new { message = "Repository already exists in your notes." });

            var newProject = new ProjectNote
            {
                UserId = userId, // Assign to user
                RepoName = repo.Name,
                PrivateNotes = repo.Description ?? "Imported from GitHub.",
                PriorityLevel = "Medium",
                Status = "Backlog",
                Language = repo.Language ?? "Unknown" 
            };
            
            _context.Projects.Add(newProject);
            await _context.SaveChangesAsync();

            return Ok(newProject);
        }

        // PUT: api/projects/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, ProjectNote project)
        {
            if (id != project.Id)
            {
                return BadRequest();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Find the existing project and verify this user owns it
            var existingProject = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            if (existingProject == null)
            {
                return NotFound();
            }

            // Update only the allowed fields
            existingProject.RepoName = project.RepoName;
            existingProject.PriorityLevel = project.PriorityLevel;
            existingProject.Status = project.Status;
            existingProject.PrivateNotes = project.PrivateNotes;
            existingProject.Language = project.Language;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/projects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Find the project and verify this user owns it before deleting
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}