namespace Backend.API.Models;

public class AnalysisResponse
{
    public List<LabFileMatch> Matches { get; set; } = new();
}