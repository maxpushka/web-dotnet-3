namespace Frontend.Blazor.Models;

public record AnalysisResponse
{
    public List<LabFileMatch> Matches { get; set; } = [];
}

public record LabFileMatch
{
    public string FileId { get; set; }
    public string DuplicateWith { get; set; }
    public List<string> DuplicatedLines { get; set; }
    public float DuplicatePercentage { get; set; }
}