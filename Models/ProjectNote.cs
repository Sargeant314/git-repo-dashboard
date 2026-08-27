namespace RepoDashboard.Models
{
    public class ProjectNote
    {
        public int Id { get; set; }
        
        public string UserId { get; set; } = string.Empty;
        
        public string RepoName { get; set; } = string.Empty;
        public string PriorityLevel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PrivateNotes { get; set; } = string.Empty;
        
        public string? Language { get; set; }
    }
}