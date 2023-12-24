using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Frontend.Blazor.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Frontend.Blazor.Code;

public interface ILoginService
{
    Task<bool> LoginAsync(LoginModel model);
    Task<List<Claim>> GetLoginInfoAsync();
    Task LogoutAsync();
    Task<string> GetAccessTokenAsync();
}

public interface IProtectedLocalStorage
{
    ValueTask<ProtectedBrowserStorageResult<TValue>> GetAsync<TValue>(string key);
    ValueTask SetAsync(string key, object value);
    ValueTask DeleteAsync(string key);
}

public interface ILocalStorage
{
    Task SetAsync<T>(string key, T value);
    Task<T> GetAsync<T>(string key);
    Task DeleteAsync(string key);
}

public interface INavigationManager
{
    void NavigateTo([StringSyntax("Uri")] string uri, bool forceLoad);
}