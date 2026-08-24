using System.Text.Json;
using System.Text.Json.Serialization;

namespace RepoDashboard.Services
{
    public class GitHubService
    {
        private readonly HttpClient _httpClient;

        public GitHubService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // GitHub API requires a User-Agent header
            _httpClient.DefaultRequestHeaders.UserAgent.Add(
                new System.Net.Http.Headers.ProductInfoHeaderValue("RepoDashboard", "1.0")
            );
        }

        public async Task<IEnumerable<GitHubRepoDto>> GetUserReposAsync(string username)
        {
            var response = await _httpClient.GetAsync($"https://api.github.com/users/{username}/repos?sort=updated&per_page=12");

            if(!response.IsSuccessStatusCode) 
            {
                return Enumerable.Empty<GitHubRepoDto>();
            }

            var jsonStream = await response.Content.ReadAsStreamAsync();
            var repos = await JsonSerializer.DeserializeAsync<IEnumerable<GitHubRepoDto>>(jsonStream);

            return repos ?? Enumerable.Empty<GitHubRepoDto>();
        }

        public async Task<GitHubRepoDto?> GetSingleRepoAsync(string username, string repoName)
        {
            var response = await _httpClient.GetAsync($"https://api.github.com/repos/{username}/{repoName}");
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var jsonStream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<GitHubRepoDto>(jsonStream);
        }
    }

    public class GitHubRepoDto
    {
        [JsonPropertyName("name")]
        public string Name {get; set;} = string.Empty;

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }
    }
}