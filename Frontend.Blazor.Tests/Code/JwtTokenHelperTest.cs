using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Frontend.Blazor.Code;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Frontend.Blazor.Tests.Code;

[TestClass]
[TestSubject(typeof(JwtTokenHelper))]
public class JwtTokenHelperTest
{
    public IConfiguration Configuration;

    [TestInitialize]
    public void Setup()
    {
        var myConfiguration = new Dictionary<string, string>
        {
            { "JWTSettings:Secret", "MyTestSecretMustBeLongEnoughAndSafeEnough" },
            { "JWTSettings:ValidIssuer", "SampleIssuer" }
        };

        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();
    }

    [TestMethod]
    public void ValidateDecodeToken_ValidToken_ReturnsClaims()
    {
        // Arrange
        var token = GenerateJwtToken();

        // Act
        var result = JwtTokenHelper.ValidateDecodeToken(token, Configuration);

        // Assert
        Assert.IsTrue(result.Count > 0);
    }

    [TestMethod]
    public void ValidateDecodeToken_InvalidToken_ReturnsEmptyClaims()
    {
        // Arrange
        var token = "invalidToken";

        // Act
        var result = JwtTokenHelper.ValidateDecodeToken(token, Configuration);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void ValidateDecodeToken_EmptyToken_ReturnsEmptyClaims()
    {
        // Arrange
        var token = string.Empty;

        // Act
        var result = JwtTokenHelper.ValidateDecodeToken(token, Configuration);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    public string GenerateJwtToken()
    {
        var securityKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("JWTSettings:Secret")));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
        var header = new JwtHeader(credentials);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "TestUser")
        };

        var payload = new JwtPayload(
            issuer: Configuration.GetValue<string>("JWTSettings:ValidIssuer"),
            audience: null,
            claims: claims,
            notBefore: DateTime.UtcNow,
            // Token will expire in 1 hour
            expires: DateTime.UtcNow.AddHours(1)
        );

        var jwt = new JwtSecurityToken(header, payload);

        var tokenHandler = new JwtSecurityTokenHandler();
        var encodedJwt = tokenHandler.WriteToken(jwt);

        return encodedJwt;
    }
}