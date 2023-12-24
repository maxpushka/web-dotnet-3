using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Backend.API.Controllers;
using Backend.API.Models;
using Backend.API.Tests.Integration.TestConfiguration;
using JetBrains.Annotations;
using Xunit;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Backend.API.Tests.Integration.Controllers;

[TestSubject(typeof(AnalyzeController))]
public class AnalyzeControllerTest(ApplicationFactory factory) : IClassFixture<ApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task TestAnalyzeLab_NoAuthorization()
    {
        // Arrange
        var request = new AnalysisInput
        {
            UserId = "c784d6e7-4424-4fe1-a1bb-b03c6a9a26cb",
            Name = "Test lab",
            FilesContent = new Dictionary<string, string> { { "Program.cs", "Q29uc29sZS5Xcml0ZUxpbmUoIkhlbGxvLCBXb3JsZCEiKTsK" } }
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/Analyze/New", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TestAnalyzeLab_LabContentIntersectionFound()
    {
        // Arrange
        var auth = await Authenticate("joedoe@gmail.com");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.JwtToken);

        var request = new AnalysisInput
        {
            UserId = "c784d6e7-4424-4fe1-a1bb-b03c6a9a26cb",
            Name = "Test lab",
            FilesContent = new Dictionary<string, string>
                { { "Program.cs", "Q29uc29sZS5Xcml0ZUxpbmUoIkhlbGxvLCBXb3JsZCEiKTsK" } }
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/Analyze/New", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var labs = await response.Content.ReadFromJsonAsync<BaseApiResponse<AnalysisResponse>>();
        Assert.IsTrue(labs.Result.Matches.Count > 0);
    }

    [Fact]
    public async Task TestGetAllLabs_NoAuthorization()
    {
        // Act
        var response = await _client.GetAsync("api/Analyze/All");

        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TestGetAllLabs_WithValidAuthorization()
    {
        // Arrange
        var auth = await Authenticate("joedoe@gmail.com");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.JwtToken);

        // Act
        var labsResp = await _client.GetAsync("api/Analyze/All");

        // Assert
        labsResp.EnsureSuccessStatusCode();
        var labs = await labsResp.Content.ReadFromJsonAsync<BaseApiResponse<LabsResponse>>();
        Assert.IsTrue(labs.Result.Labs.Count > 0);
    }

    private async Task<AuthenticationResponse> Authenticate(string email)
    {
        var authResp = await _client.PostAsJsonAsync("api/account/login", new LoginInput
        {
            Email = email,
            Password = "Mil@d1234"
        });
        authResp.EnsureSuccessStatusCode();
        var auth = await authResp.Content.ReadFromJsonAsync<BaseApiResponse<AuthenticationResponse>>();
        return auth.Result;
    }
}