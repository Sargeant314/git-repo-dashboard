namespace git-repo-dashboard
{
    public class ProjectNote
    {
        public int Id {get; set;}
        public string RepoName {get; set;}
        public string PriorityLevel {get; set;} //High, Med, Low
        public string Status {get; set;}
        public string PrivateNotes { get; set; }
    }
}