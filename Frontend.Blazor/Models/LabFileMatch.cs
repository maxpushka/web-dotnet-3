namespace Frontend.Blazor.Models;

public class LabFileMatch
{
    public string FileId { get; set; }
    public string DuplicateWith { get; set; }
    public List<string> DuplicatedLines { get; set; }
}