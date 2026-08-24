using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepoDashboard.Models;
using RepoDashboard.Data;

namespace RepoDashboard.Controllers
{
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
            return await _context.Projects.ToListAsync();
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
            var repos = await gitHubService.GetUserReposAsync(username);
            return Ok(repos);
        }

        // POST: api/projects
        [HttpPost]
        public async Task<ActionResult<ProjectNote>> PostProject(ProjectNote project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProjects), new {id = project.Id }, project);
        }

        // POST: api/projects/import-github/{username}
        [HttpPost("import-github/{username}")]
        public async Task<IActionResult> BulkImportGitHubRepos(
            string username, 
            [FromServices] RepoDashboard.Services.GitHubService gitHubService)
        {
            var repos = await gitHubService.GetUserReposAsync(username);
            int addedCount = 0;

            foreach (var repo in repos)
            {
                bool exists = await _context.Projects.AnyAsync(p => p.RepoName.ToLower() == repo.Name.ToLower());
                if (!exists)
                {
                    _context.Projects.Add(new ProjectNote
                    {
                        RepoName = repo.Name,
                        PrivateNotes = repo.Description ?? "Imported from GitHub.",
                        PriorityLevel = "Medium",
                        Status = "Backlog",
                        Language = repo.Language ?? "Unknown" // Save Language
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
            var repo = await gitHubService.GetSingleRepoAsync(username, repoName);
            if (repo == null) return NotFound(new { message = "Repository not found on GitHub." });

            bool exists = await _context.Projects.AnyAsync(p => p.RepoName.ToLower() == repo.Name.ToLower());
            if (exists) return Conflict(new { message = "Repository already exists in your notes." });

            var newProject = new ProjectNote
            {
                RepoName = repo.Name,
                PrivateNotes = repo.Description ?? "Imported from GitHub.",
                PriorityLevel = "Medium",
                Status = "Backlog",
                Language = repo.Language ?? "Unknown" // Save Language
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

            _context.Entry(project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch 
            {
                if(!_context.Projects.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/projects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if(project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}