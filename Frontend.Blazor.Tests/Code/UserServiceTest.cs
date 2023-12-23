using Frontend.Blazor.Code;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Frontend.Blazor.HttpClients;
using Frontend.Blazor.Models;
using System.Net.Http;

namespace Frontend.Blazor.Tests.Code;

[TestClass]
[TestSubject(typeof(UserService))]
public class UserServiceTest
{
  private Mock<IBackendApiHttpClient> _backendApiHttpClientMock;
  private UserService _userService;

  [TestInitialize]
  public void TestInitialize()
  {
    _backendApiHttpClientMock = new Mock<IBackendApiHttpClient>();
    _userService = new UserService(_backendApiHttpClientMock.Object);
  }

  [TestMethod]
  public async Task GetAllUsers_WhenCalled_ReturnsAllUsers()
  {
    var authToken = "testAuthToken";
    var usersResponse = new UsersResponse();
    _backendApiHttpClientMock.Setup(b => b.GetAllUsersAsync(authToken, null)).ReturnsAsync(
      new ApiResponse<UsersResponse>
      {
        Result = usersResponse
      });

    var result = await _userService.GetAllUsers(authToken);

    Assert.AreEqual(usersResponse, result);
    _backendApiHttpClientMock.Verify(b => b.GetAllUsersAsync(authToken, null), Times.Once);
  }

  [TestMethod]
  public async Task GetAllUsers_WhenCalledWithErrors_ThrowsHttpRequestException()
  {
    var authToken = "testAuthToken";
    _backendApiHttpClientMock.Setup(b => b.GetAllUsersAsync(authToken, null)).ReturnsAsync(
      new ApiResponse<UsersResponse>
      {
        Errors = ["Test error"]
      });

    await Assert.ThrowsExceptionAsync<HttpRequestException>(() => _userService.GetAllUsers(authToken));

    _backendApiHttpClientMock.Verify(b => b.GetAllUsersAsync(authToken, null), Times.Once);
  }
}