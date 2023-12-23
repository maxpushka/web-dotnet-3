using Frontend.Blazor.Code;
using Frontend.Blazor.HttpClients;
using Frontend.Blazor.Models;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;

namespace Frontend.Blazor.Tests.Code;

[TestClass]
[TestSubject(typeof(LoginService))]
public class LoginServiceTest
{
    private Mock<IBackendApiHttpClient> _backendApiHttpClientMock;
    private Mock<INavigationManager> _navigationManagerMock;
    private Mock<IConfiguration> _configurationMock;
    private Mock<ILocalStorage> _localStorageMock;

    [TestInitialize]
    public void Initialize()
    {
        _backendApiHttpClientMock = new Mock<IBackendApiHttpClient>();
        _navigationManagerMock = new Mock<INavigationManager>();
        _configurationMock = new Mock<IConfiguration>();
        _localStorageMock = new Mock<ILocalStorage>();
    }

    [TestMethod]
    public async Task LoginAsync_ReturnsFalse_WhenLoginFails()
    {
        // Arrange
        _backendApiHttpClientMock.Setup(x => x.LoginUserAsync(It.IsAny<LoginModel>(), null))
            .Returns(Task.FromResult(default(ApiResponse<AuthResponse>)));
        var service = new LoginService(_localStorageMock.Object, _navigationManagerMock.Object,
            _configurationMock.Object, _backendApiHttpClientMock.Object);

        // Act
        var loginResult = await service.LoginAsync(new LoginModel());

        // Assert
        Assert.IsFalse(loginResult);
    }

    [TestMethod]
    public async Task LoginAsync_ReturnsTrue_WhenLoginSucceeds()
    {
        // Arrange
        var response = new ApiResponse<AuthResponse>()
            { Result = new AuthResponse() { JwtToken = "testToken" }, Errors = null };
        _backendApiHttpClientMock.Setup(x => x.LoginUserAsync(It.IsAny<LoginModel>(), null))
            .Returns(Task.FromResult(response));
        var service = new LoginService(_localStorageMock.Object, _navigationManagerMock.Object,
            _configurationMock.Object, _backendApiHttpClientMock.Object);

        // Act
        var loginResult = await service.LoginAsync(new LoginModel());

        // Assert
        Assert.IsTrue(loginResult);
    }

    [TestMethod]
    public async Task GetLoginInfoAsync_ReturnsEmpty_WhenNoTokenIsStored()
    {
        // Arrange
        _localStorageMock.Setup(x => x.GetAsync<string>(It.IsAny<string>())).Throws<CryptographicException>();
        var service = new LoginService(_localStorageMock.Object, _navigationManagerMock.Object,
            _configurationMock.Object, _backendApiHttpClientMock.Object);

        // Act
        var loginInfo = await service.GetLoginInfoAsync();

        // Assert
        Assert.IsFalse(loginInfo.Any());
    }

    [TestMethod]
    public async Task GetLoginInfoAsync_ReturnsClaims_WhenAccessTokenIsValid()
    {
        // Arrange
        var jwtTokenHelper = new JwtTokenHelperTest();
        jwtTokenHelper.Setup();
        var validToken = jwtTokenHelper.GenerateJwtToken();

        _localStorageMock.Setup(x => x.GetAsync<string>(It.IsAny<string>())).ReturnsAsync(validToken);
        var service = new LoginService(_localStorageMock.Object, _navigationManagerMock.Object,
            jwtTokenHelper.Configuration, _backendApiHttpClientMock.Object);

        // Act
        var result = await service.GetLoginInfoAsync();

        // Assert
        Assert.IsTrue(result.Count > 0);
    }

    [TestMethod]
    public async Task GetLoginInfoAsync_ReturnsClaims_RefreshAccessTokenWhenExpired()
    {
        // Arrange
        var jwtTokenHelper = new JwtTokenHelperTest();
        jwtTokenHelper.Setup();
        var validToken = jwtTokenHelper.GenerateJwtToken();
        _backendApiHttpClientMock.Setup(x => x.RefreshTokenAsync(It.IsAny<string>(), null))
            .Returns(Task.FromResult(new ApiResponse<AuthResponse>
                { Result = new AuthResponse { JwtToken = validToken } }));
        _localStorageMock.Setup(x => x.GetAsync<string>(It.IsAny<string>())).ReturnsAsync("InvalidToken");
        _localStorageMock.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>()));

        var service = new LoginService(_localStorageMock.Object, _navigationManagerMock.Object,
            jwtTokenHelper.Configuration, _backendApiHttpClientMock.Object);

        // Act
        var result = await service.GetLoginInfoAsync();

        // Assert
        Assert.IsTrue(result.Count > 0);
    }

    [TestMethod]
    public async Task GetAccessTokenAsync_ThrowsException_WhenAccessTokenIsEmpty()
    {
        // Arrange
        _localStorageMock.Setup(x => x.GetAsync<string>(It.IsAny<string>())).ReturnsAsync(null as string);
        var service = new LoginService(_localStorageMock.Object, _navigationManagerMock.Object,
            _configurationMock.Object, _backendApiHttpClientMock.Object);

        // Act && Assert
        await Assert.ThrowsExceptionAsync<AuthenticationFailureException>(() => service.GetAccessTokenAsync());
    }

    [TestMethod]
    public async Task GetAccessTokenAsync_ReturnsAccessToken_WhenAccessTokenIsValid()
    {
        // Arrange
        var token = "Token";
        _localStorageMock.Setup(x => x.GetAsync<string>(It.IsAny<string>())).ReturnsAsync(token);
        var service = new LoginService(_localStorageMock.Object, _navigationManagerMock.Object,
            _configurationMock.Object, _backendApiHttpClientMock.Object);

        // Act
        var accessToken = await service.GetAccessTokenAsync();

        // Assert
        Assert.AreEqual(token, accessToken);
    }

    [TestMethod]
    public async Task LogoutAsync_ClearsStorage()
    {
        // Arrange
        var service = new LoginService(_localStorageMock.Object, _navigationManagerMock.Object,
            _configurationMock.Object, _backendApiHttpClientMock.Object);

        // Act
        await service.LogoutAsync();

        // Assert
        _localStorageMock.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Exactly(2));
        _navigationManagerMock.Verify(x => x.NavigateTo(It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
    }
}