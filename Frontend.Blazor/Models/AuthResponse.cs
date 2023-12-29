namespace Frontend.Blazor.Models;

public record AuthResponse
{
    public string JwtToken { get; set; }
    public string RefreshToken { get; set; }
}