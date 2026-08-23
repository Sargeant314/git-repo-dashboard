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

        // POST: api/projects
        [HttpPost]
        public async Task<ActionResult<ProjectNote>> PostProject(ProjectNote project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProjects), new {id = project.Id }, project);
        }
    }
}