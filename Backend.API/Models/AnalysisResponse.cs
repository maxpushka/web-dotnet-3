namespace Backend.API.Models;

public record AnalysisResponse
{
    public List<LabFileMatch> Matches { get; set; } = new();
}