using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Cryptography;
using Frontend.Blazor.HttpClients;
using Frontend.Blazor.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Frontend.Blazor.Code;

public class LoginService(
    ILocalStorage localStorage,
    INavigationManager navigation,
    IConfiguration configuration,
    IBackendApiHttpClient backendApiHttpClient)
    : ILoginService
{
    private const string AccessToken = nameof(AccessToken);
    private const string RefreshToken = nameof(RefreshToken);

    public async Task<bool> LoginAsync(LoginModel model)
    {
        var response = await backendApiHttpClient.LoginUserAsync(model);
        if (string.IsNullOrEmpty(response?.Result?.JwtToken))
            return false;

        await localStorage.SetAsync(AccessToken, response.Result.JwtToken);
        await localStorage.SetAsync(RefreshToken, response.Result.RefreshToken);

        return true;
    }


    public async Task<List<Claim>> GetLoginInfoAsync()
    {
        var emptyResult = new List<Claim>();
        string accessToken;
        string refreshToken;
        try
        {
            accessToken = await localStorage.GetAsync<string>(AccessToken);
            refreshToken = await localStorage.GetAsync<string>(RefreshToken);
        }
        catch (CryptographicException)
        {
            await LogoutAsync();
            return emptyResult;
        }

        if (accessToken == default)
            return emptyResult;

        var claims = JwtTokenHelper.ValidateDecodeToken(accessToken, configuration);

        if (claims.Count != 0)
            return claims;

        if (refreshToken != default)
        {
            var response = await backendApiHttpClient.RefreshTokenAsync(refreshToken);
            if (string.IsNullOrWhiteSpace(response?.Result?.JwtToken) is false)
            {
                await localStorage.SetAsync(AccessToken, response.Result.JwtToken);
                await localStorage.SetAsync(RefreshToken, response.Result.RefreshToken);
                claims = JwtTokenHelper.ValidateDecodeToken(response.Result.JwtToken, configuration);
                return claims;
            }

            await LogoutAsync();
        }
        else
        {
            await LogoutAsync();
        }

        return claims;
    }

    public async Task LogoutAsync()
    {
        await RemoveAuthDataFromStorageAsync();
        navigation.NavigateTo("/", true);
    }

    private async Task RemoveAuthDataFromStorageAsync()
    {
        await localStorage.DeleteAsync(AccessToken);
        await localStorage.DeleteAsync(RefreshToken);
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var accessToken = await localStorage.GetAsync<string>(AccessToken);
        if (accessToken == default)
        {
            throw new AuthenticationFailureException("Access token not found");
        }

        return accessToken;
    }
}

public class LocalStorageWrapper(IProtectedLocalStorage localStorage) : ILocalStorage
{
    public async Task SetAsync<T>(string key, T value)
    {
        await localStorage.SetAsync(key, value);
    }

    public async Task<T> GetAsync<T>(string key)
    {
        var storageResult = await localStorage.GetAsync<T>(key);
        return !storageResult.Success ? default : storageResult.Value;
    }

    public async Task DeleteAsync(string key)
    {
        await localStorage.DeleteAsync(key);
    }
}

public class ProtectedLocalStorageWrapper(ProtectedLocalStorage localStorage) : IProtectedLocalStorage
{
    public async ValueTask<ProtectedBrowserStorageResult<TValue>> GetAsync<TValue>(string key)
    {
        return await localStorage.GetAsync<TValue>(key);
    }

    public async ValueTask SetAsync(string key, object value)
    {
        await localStorage.SetAsync(key, value);
    }

    public async ValueTask DeleteAsync(string key)
    {
        await localStorage.DeleteAsync(key);
    }
}

public class NavigationManagerWrapper(NavigationManager navigationManager) : INavigationManager
{
    public void NavigateTo([StringSyntax("Uri")] string uri, bool forceLoad)
    {
        navigationManager.NavigateTo(uri, forceLoad);
    }
}