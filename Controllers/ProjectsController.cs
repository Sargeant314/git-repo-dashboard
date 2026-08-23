using Microsoft.AspNetCore.Mvc;
using git-repo-dashboard.Models;

namespace git-repo-dashboard.Controllers
{
    [APIController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetProject()
        [
            var sampleProject = new ProjectNote
            {
                Id = 1,
                RepoName = "PWA-Checklist",
                PriorityLevel= "Medium",
                Status = "Completed",
                PrivateNotes = "Needs some polish"
            };
            return Ok(sampleProject);
        ]
    }
}