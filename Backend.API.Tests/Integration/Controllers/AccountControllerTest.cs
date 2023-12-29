using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Backend.API.Controllers;
using Backend.API.Models;
using Backend.API.Permissions;
using Backend.API.Tests.Integration.TestConfiguration;
using JetBrains.Annotations;
using Xunit;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Backend.API.Tests.Integration.Controllers;

[TestSubject(typeof(AccountController))]
public class AccountControllerTest(ApplicationFactory factory) : IClassFixture<ApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

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

    [Fact]
    public async Task TestRefresh()
    {
        // Arrange
        var auth = await Authenticate("joedoe@gmail.com");
        var request = new RefreshTokenInput { RefreshToken = auth.RefreshToken };

        // Act
        var refreshResp = await _client.PostAsJsonAsync("api/account/refresh", request);

        // Assert
        refreshResp.EnsureSuccessStatusCode();
        var refresh = await refreshResp.Content.ReadFromJsonAsync<BaseApiResponse<AuthenticationResponse>>();
        Assert.IsNotNull(refresh);
        Assert.IsNull(refresh.Errors);

        Assert.IsNotNull(refresh.Result);
        Assert.IsNotNull(refresh.Result.JwtToken);
        Assert.IsNotNull(refresh.Result.RefreshToken);
    }

    [Fact]
    public async Task TestLogin()
    {
        // Arrange
        var request = new LoginInput
        {
            Email = "joedoe@gmail.com",
            Password = "Mil@d1234"
        };

        // Act
        var authResp = await _client.PostAsJsonAsync("api/account/login", request);

        // Assert
        authResp.EnsureSuccessStatusCode();

        var auth = await authResp.Content.ReadFromJsonAsync<BaseApiResponse<AuthenticationResponse>>();
        Assert.IsNotNull(auth);
        Assert.IsNull(auth.Errors);

        Assert.IsNotNull(auth.Result);
        Assert.IsNotNull(auth.Result.JwtToken);
        Assert.IsNotNull(auth.Result.RefreshToken);
    }

    [Fact]
    public async Task TestRegister_Success()
    {
        // Arrange
        var auth = await Authenticate("admin@gmail.com");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.JwtToken);

        var request = new UserRegisterInput
        {
            Email = "hello@world.com",
            Mobile = "0991234567",
            Name = "Tester",
            Family = "Testify",
            UserName = "hello@world.com",
            Password = "Mil@d1234",
            ConfirmPassword = "Mil@d1234",
            Role = ["user"]
        };

        // Act
        var registerResp = await _client.PostAsJsonAsync("api/account/register", request);

        // Assert
        registerResp.EnsureSuccessStatusCode();

        var register = await registerResp.Content.ReadFromJsonAsync<BaseApiResponse<string>>();
        Assert.AreEqual("OK", register.Result);
    }

    [Fact]
    public async Task TestRegister_AccountAlreadyExist()
    {
        // Arrange
        var auth = await Authenticate("admin@gmail.com");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.JwtToken);

        var request = new UserRegisterInput
        {
            Email = "joedoe@gmail.com",
            Mobile = "0963233542",
            Name = "Joe",
            Family = "Doe",
            UserName = "joedoe@gmail.com",
            Password = "Mil@d1234",
            ConfirmPassword = "Mil@d1234",
            Role = ["user"]
        };

        // Act
        var registerResp = await _client.PostAsJsonAsync("api/account/register", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, registerResp.StatusCode);
    }

    [Fact]
    private async Task TestDisableUser_Success()
    {
        // Arrange
        var auth = await Authenticate("admin@gmail.com");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.JwtToken);

        const string userId = "c784d6e7-4424-4fe1-a1bb-b03c6a9a26cb";

        // Act
        var disableResp = await _client.DeleteAsync($"api/account/user/{userId}");

        // Assert
        disableResp.EnsureSuccessStatusCode();

        var disable = await disableResp.Content.ReadFromJsonAsync<BaseApiResponse<string>>();
        Assert.AreEqual("done", disable.Result);
    }

    [Fact]
    private async Task TestDisableUser_UserNotFound()
    {
        // Arrange
        var auth = await Authenticate("admin@gmail.com");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.JwtToken);

        const string userId = "invalid-id";

        // Act
        var disableResp = await _client.DeleteAsync($"api/account/user/{userId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, disableResp.StatusCode);

        var disable = await disableResp.Content.ReadFromJsonAsync<BaseApiResponse<string>>();
        Assert.IsNull(disable.Result);
        Assert.IsNotNull(disable.Errors);
    }

    [Fact]
    private async Task TestGetUser_Success()
    {
        // Arrange
        var auth = await Authenticate("admin@gmail.com");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.JwtToken);

        const string userId = "c784d6e7-4424-4fe1-a1bb-b03c6a9a26cb";

        // Act
        var resp = await _client.GetAsync($"api/account/user/{userId}");

        // Assert
        resp.EnsureSuccessStatusCode();

        var user = await resp.Content.ReadFromJsonAsync<BaseApiResponse<UserVM>>();
        Assert.AreEqual(userId, user.Result.Id);
        Assert.IsNull(user.Errors);
    }

    [Fact]
    private async Task TestGetUser_UserNotFound()
    {
        // Arrange
        var auth = await Authenticate("admin@gmail.com");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.JwtToken);

        const string userId = "invalid-id";

        // Act
        var resp = await _client.GetAsync($"api/account/user/{userId}");

        // Assert
        resp.EnsureSuccessStatusCode();

        var user = await resp.Content.ReadFromJsonAsync<BaseApiResponse<UserVM>>();
        Assert.IsNull(user.Result);
        Assert.IsNull(user.Errors);
    }

    [Fact]
    private async Task TestGetAllPermissions()
    {
        // Arrange
        var auth = await Authenticate("admin@gmail.com");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.JwtToken);

        // Act
        var permissionsResp = await _client.GetAsync("api/account/permission");

        // Assert
        permissionsResp.EnsureSuccessStatusCode();

        var permissions = await permissionsResp.Content.ReadFromJsonAsync<BaseApiResponse<PermissionList>>();
        Assert.IsNotNull(permissions.Result);
        Assert.IsTrue(permissions.Result.Permissions.Count > 0);
        Assert.IsNull(permissions.Errors);
    }


    [Fact]
    private async Task TestCreateRole()
    {
        // Arrange
        var auth = await Authenticate("admin@gmail.com");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.JwtToken);

        var request = new RoleInput
        {
            Name = "test",
            Permissions =
            [
                new PermissionInput
                {
                    Value = "Administrative.ViewUsers",
                    IsChecked = true
                }
            ]
        };

        // Act
        var rolesResp = await _client.PostAsJsonAsync("api/account/role", request);

        // Assert
        rolesResp.EnsureSuccessStatusCode();

        var roles = await rolesResp.Content.ReadFromJsonAsync<BaseApiResponse<List<string>>>();
        Assert.IsNotNull(roles.Result);
        Assert.IsNull(roles.Errors);
    }

    [Fact]
    private async Task TestGetAllRoles()
    {
        // Arrange
        var auth = await Authenticate("admin@gmail.com");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.JwtToken);

        // Act
        var rolesResp = await _client.GetAsync("api/account/role");

        // Assert
        rolesResp.EnsureSuccessStatusCode();

        var roles = await rolesResp.Content.ReadFromJsonAsync<BaseApiResponse<List<RoleItem>>>();
        Assert.IsNotNull(roles.Result);
        Assert.IsNull(roles.Errors);
    }

    [Fact]
    private async Task TestUpdateRole()
    {
        // Arrange
        var auth = await Authenticate("admin@gmail.com");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.JwtToken);

        const string roleId = "03B11179-8A33-4D3B-8092-463249F755A5";
        var request = new RoleInput
        {
            Name = "user",
            Permissions =
            [
                new PermissionInput
                {
                    Value = "Administrative.ManageUsers",
                    IsChecked = true
                }
            ]
        };

        // Act
        var rolesResp = await _client.PutAsJsonAsync($"api/account/role/{roleId}", request);

        // Assert
        rolesResp.EnsureSuccessStatusCode();

        var roles = await rolesResp.Content.ReadFromJsonAsync<BaseApiResponse<bool>>();
        Assert.IsTrue(roles.Result);
        Assert.IsNull(roles.Errors);
    }

    [Fact]
    private async Task TestGetAllUsers()
    {
        // Arrange
        var auth = await Authenticate("admin@gmail.com");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.JwtToken);

        // Act
        var usersResp = await _client.GetAsync("api/account/user");

        // Assert
        usersResp.EnsureSuccessStatusCode();

        var users = await usersResp.Content.ReadFromJsonAsync<BaseApiResponse<UserListVM>>();
        Assert.IsTrue(users.Result.Users.Count > 0);
        Assert.IsNull(users.Errors);
    }

    [Fact]
    private async Task TestUpdateUser_Success()
    {
        // Arrange
        var auth = await Authenticate("admin@gmail.com");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.JwtToken);

        const string userId = "c784d6e7-4424-4fe1-a1bb-b03c6a9a26cb";
        var request = new UpdateUserInput
        {
            Mobile = "0991234567",
            Name = "Alex",
            Family = "Doe",
            Role = ["user"]
        };

        // Act
        var usersResp = await _client.PutAsJsonAsync($"api/account/user/{userId}", request);

        // Assert
        usersResp.EnsureSuccessStatusCode();

        var users = await usersResp.Content.ReadFromJsonAsync<BaseApiResponse<bool>>();
        Assert.IsTrue(users.Result);
        Assert.IsNull(users.Errors);
    }

    [Fact]
    private async Task TestUpdateUser_UserDoesNotExist()
    {
        // Arrange
        var auth = await Authenticate("admin@gmail.com");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.JwtToken);

        const string userId = "invalid-id";
        var request = new UpdateUserInput
        {
            Mobile = "0991234567",
            Name = "Alex",
            Family = "Doe",
            Role = ["user"]
        };

        // Act
        var usersResp = await _client.PutAsJsonAsync($"api/account/user/{userId}", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, usersResp.StatusCode);

        var users = await usersResp.Content.ReadFromJsonAsync<BaseApiResponse<bool>>();
        Assert.IsFalse(users.Result);
        Assert.IsNull(users.Errors);
    }
}