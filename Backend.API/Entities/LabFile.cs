using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.API.Entities;

public record LabFile
{
    [Key] public string Id { get; set; }

    [Required] public string LabId { get; set; }

    [ForeignKey("LabId")] public virtual Lab Lab { get; set; }
    
    [Required] public string Name { get; set; }

    [Required] public string FileContent { get; set; }
}