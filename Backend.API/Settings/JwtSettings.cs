namespace Backend.API.Settings;

public record JwtSettings
{
    public string Secret { get; set; }
    public string Issuer { get; set; }
    public int JWTExpirationTime { get; set; }
    public int RefreshExpirationTime { get; set; }
}