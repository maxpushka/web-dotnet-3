using Frontend.Blazor.Code;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frontend.Blazor.HttpClients;
using Frontend.Blazor.Models;

namespace Frontend.Blazor.Tests.Code;

[TestClass]
[TestSubject(typeof(LabService))]
public class LabServiceTest
{
    private Mock<IBackendApiHttpClient> _mockBackendApiHttpClient;
    private LabService _labService;

    [TestInitialize]
    public void Initialize()
    {
        _mockBackendApiHttpClient = new Mock<IBackendApiHttpClient>();
        _labService = new LabService(_mockBackendApiHttpClient.Object);
    }

    [TestMethod]
    public async Task AnalyzeLab_Successful_AnalysisResponseReturned()
    {
        // Arrange
        var authToken = "testToken";
        var analysisRequest = new AnalysisRequest { UserId = "userId", Name = "testName" };
        var expectedResponse = new ApiResponse<AnalysisResponse> { Result = new AnalysisResponse() };

        _mockBackendApiHttpClient
            .Setup(client => client.AnalyzeLabAsync(authToken, analysisRequest, It.IsAny<CancellationToken?>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _labService.AnalyzeLab(authToken, analysisRequest);

        // Assert
        Assert.AreEqual(expectedResponse.Result, result);
    }

    [TestMethod]
    public async Task AnalyzeLab_Unsuccessful_HttpRequestExceptionThrown()
    {
        // Arrange
        var authToken = "testToken";
        var analysisRequest = new AnalysisRequest { UserId = "userId", Name = "testName" };
        var expectedResponse = new ApiResponse<AnalysisResponse> { Errors = new List<string> { "error" } };

        _mockBackendApiHttpClient
            .Setup(client => client.AnalyzeLabAsync(authToken, analysisRequest, It.IsAny<CancellationToken?>()))
            .ReturnsAsync(expectedResponse);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _labService.AnalyzeLab(authToken, analysisRequest));
    }

    [TestMethod]
    public async Task GetAllLabsAsync_Successful_LabsResponseReturned()
    {
        // Arrange
        var authToken = "testToken";
        var expectedResponse = new ApiResponse<LabsResponse> { Result = new LabsResponse() };

        _mockBackendApiHttpClient
            .Setup(client => client.GetAllLabsAsync(authToken, It.IsAny<CancellationToken?>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _labService.GetAllLabsAsync(authToken);

        // Assert
        Assert.AreEqual(expectedResponse.Result, result);
    }

    [TestMethod]
    public async Task GetAllLabsAsync_Unsuccessful_HttpRequestExceptionThrown()
    {
        // Arrange
        var authToken = "testToken";
        var expectedResponse = new ApiResponse<LabsResponse> { Errors = new List<string> { "error" } };

        _mockBackendApiHttpClient
            .Setup(client => client.GetAllLabsAsync(authToken, It.IsAny<CancellationToken?>()))
            .ReturnsAsync(expectedResponse);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _labService.GetAllLabsAsync(authToken));
    }
}