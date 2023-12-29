using System.Collections.Generic;
using Frontend.Blazor.Code;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Frontend.Blazor.Tests.Code;

[TestClass]
[TestSubject(typeof(CustomAuthStateProvider))]
public class CustomAuthStateProviderTest
{
    // Test the GetAuthenticationStateAsync function
    [TestMethod]
    public async Task GetAuthenticationStateAsync_Test()
    {
        // Arrange
        var mockLoginService = new Mock<ILoginService>();
        mockLoginService.Setup(ls => ls.GetLoginInfoAsync())
            .ReturnsAsync([new Claim(ClaimTypes.Name, "TestUser")]);

        var customAuthStateProvider = new CustomAuthStateProvider(mockLoginService.Object);
        // Act
        var authState = await customAuthStateProvider.GetAuthenticationStateAsync();
        // Assert
        Assert.IsNotNull(authState);
        Assert.IsTrue(authState.User.Identity!.IsAuthenticated);
    }

    [TestMethod]
    public async Task GetAuthenticationStateAsync_NoClaims_Test()
    {
        // Arrange
        var mockLoginService = new Mock<ILoginService>();
        mockLoginService.Setup(ls => ls.GetLoginInfoAsync())
            .ReturnsAsync([]);

        var customAuthStateProvider = new CustomAuthStateProvider(mockLoginService.Object);
        // Act
        var authState = await customAuthStateProvider.GetAuthenticationStateAsync();
        // Assert
        Assert.IsNotNull(authState);
        Assert.IsFalse(authState.User.Identity!.IsAuthenticated);
    }

    [TestMethod]
    public async Task GetAuthenticationStateAsync_NullClaims_Test()
    {
        // Arrange
        var mockLoginService = new Mock<ILoginService>();
        mockLoginService.Setup(ls => ls.GetLoginInfoAsync())
            .ReturnsAsync((List<Claim>)null);

        var customAuthStateProvider = new CustomAuthStateProvider(mockLoginService.Object);
        // Act
        var authState = await customAuthStateProvider.GetAuthenticationStateAsync();
        // Assert
        Assert.IsNotNull(authState);
        Assert.IsFalse(authState.User.Identity!.IsAuthenticated);
    }
}