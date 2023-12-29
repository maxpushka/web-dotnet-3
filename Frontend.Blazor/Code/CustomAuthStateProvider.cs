using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;

namespace Frontend.Blazor.Code;

public class CustomAuthStateProvider(ILoginService loginService) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var claims = await loginService.GetLoginInfoAsync();
        var claimsIdentity = claims != null && claims.Count != 0
            ? new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme, "name", "role")
            : new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        return new AuthenticationState(claimsPrincipal);
    }
}