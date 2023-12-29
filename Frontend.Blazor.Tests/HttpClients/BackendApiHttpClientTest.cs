using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Frontend.Blazor.HttpClients;
using Frontend.Blazor.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace Frontend.Blazor.Tests.HttpClients;

[TestClass]
public class BackendApiHttpClientTests
{
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private HttpClient _mockHttpClient;

    [TestInitialize]
    public void SetUp()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockHttpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://test.com/") // Set a base address for HttpClient
        };
    }

    private void SetupMockResponse(string requestUri, HttpMethod method, HttpStatusCode statusCode, string content)
    {
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method && req.RequestUri.ToString().EndsWith(requestUri)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }

    [TestMethod]
    public async Task RegisterUserAsync_ShouldReturnSuccessResponse()
    {
        var input = new UserRegisterInput();
        var expectedResponse = new ApiResponse<string>();
        SetupMockResponse("api/account", HttpMethod.Post, HttpStatusCode.OK,
            JsonSerializer.Serialize(expectedResponse));

        var client = new BackendApiHttpClient(_mockHttpClient);

        var actualResponse = await client.RegisterUserAsync(input);

        Assert.IsNotNull(actualResponse);
        Assert.AreEqual(expectedResponse.Result, actualResponse.Result);
        Assert.AreEqual(expectedResponse.Errors, actualResponse.Errors);
    }

    [TestMethod]
    public async Task LoginUserAsync_ShouldReturnSuccessResponse()
    {
        var input = new LoginModel();
        var expectedResponse = new ApiResponse<AuthResponse>();
        SetupMockResponse("api/account/login", HttpMethod.Post, HttpStatusCode.OK,
            JsonSerializer.Serialize(expectedResponse));

        var client = new BackendApiHttpClient(_mockHttpClient);

        var actualResponse = await client.LoginUserAsync(input);

        Assert.IsNotNull(actualResponse);
        Assert.AreEqual(expectedResponse.Result, actualResponse.Result);
        Assert.AreEqual(expectedResponse.Errors, actualResponse.Errors);
    }

    [TestMethod]
    public async Task RefreshTokenAsync_ShouldReturnSuccessResponse()
    {
        var refreshToken = "some_refresh_token";
        var expectedResponse = new ApiResponse<AuthResponse>();
        SetupMockResponse("api/account/refresh", HttpMethod.Post, HttpStatusCode.OK,
            JsonSerializer.Serialize(expectedResponse));

        var client = new BackendApiHttpClient(_mockHttpClient);

        var actualResponse = await client.RefreshTokenAsync(refreshToken);

        Assert.IsNotNull(actualResponse);
        Assert.AreEqual(expectedResponse.Result, actualResponse.Result);
        Assert.AreEqual(expectedResponse.Errors, actualResponse.Errors);
    }

    [TestMethod]
    public async Task AnalyzeLabAsync_ShouldReturnSuccessResponse()
    {
        var authToken = "some_auth_token";
        var analysisRequest = new AnalysisRequest();
        var expectedResponse = new ApiResponse<AnalysisResponse>();
        SetupMockResponse("api/Analyze/New", HttpMethod.Post, HttpStatusCode.OK,
            JsonSerializer.Serialize(expectedResponse));

        var client = new BackendApiHttpClient(_mockHttpClient);

        var actualResponse = await client.AnalyzeLabAsync(authToken, analysisRequest);

        Assert.IsNotNull(actualResponse);
        Assert.AreEqual(expectedResponse.Result, actualResponse.Result);
        Assert.AreEqual(expectedResponse.Errors, actualResponse.Errors);
    }

    [TestMethod]
    public async Task GetAllUsersAsync_ShouldReturnSuccessResponse()
    {
        var authToken = "some_auth_token";
        var expectedResponse = new ApiResponse<UsersResponse>();
        SetupMockResponse("api/account/user", HttpMethod.Get, HttpStatusCode.OK,
            JsonSerializer.Serialize(expectedResponse));

        var client = new BackendApiHttpClient(_mockHttpClient);

        var actualResponse = await client.GetAllUsersAsync(authToken);

        Assert.IsNotNull(actualResponse);
        Assert.AreEqual(expectedResponse.Result, actualResponse.Result);
        Assert.AreEqual(expectedResponse.Errors, actualResponse.Errors);
    }

    [TestMethod]
    public async Task GetAllLabsAsync_ShouldReturnSuccessResponse()
    {
        var authToken = "some_auth_token";
        var expectedResponse = new ApiResponse<LabsResponse>();
        SetupMockResponse("api/Analyze/All", HttpMethod.Get, HttpStatusCode.OK,
            JsonSerializer.Serialize(expectedResponse));

        var client = new BackendApiHttpClient(_mockHttpClient);

        var actualResponse = await client.GetAllLabsAsync(authToken);

        Assert.IsNotNull(actualResponse);
        Assert.AreEqual(expectedResponse.Result, actualResponse.Result);
        Assert.AreEqual(expectedResponse.Errors, actualResponse.Errors);
    }
}