namespace Frontend.Blazor.Models;

public record LabsResponse
{
    public List<LabVM> Labs { get; set; } = new();
}

public record LabVM
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string UserId { get; set; }
    public DateTime SubmissionDate { get; set; }
    public uint NumberOfFiles { get; set; }
}