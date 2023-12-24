namespace Backend.API.Models;

public record AnalysisInput
{
    public string UserId { get; set; }

    public string Name { get; set; }

    public Dictionary<string, string> FilesContent { get; set; }
}