using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.API.Entities;

public record Lab
{
    [Key] public string Id { get; set; }

    [Required] public string UserId { get; set; }

    [ForeignKey("UserId")] public virtual ApplicationUser User { get; set; }

    [Required] public string Name { get; set; }

    [Required] public DateTime SubmissionDate { get; set; } = DateTime.Now;
}