using System.ComponentModel.DataAnnotations;

namespace Backend.API.Models;

public record LoginInput
{
    [Required] public string Email { get; set; }

    [Required] public string Password { get; set; }
}