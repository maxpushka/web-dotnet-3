namespace Backend.API.Models;

public record AuthenticationResponse
{
    public string JwtToken { get; set; }
    public string RefreshToken { get; set; }
}