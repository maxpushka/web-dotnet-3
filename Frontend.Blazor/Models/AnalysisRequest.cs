namespace Frontend.Blazor.Models;

public class AnalysisRequest
{
    public string UserId { get; set; }

    public string Name { get; set; }

    public Dictionary<string, string> FilesContent { get; set; }
}