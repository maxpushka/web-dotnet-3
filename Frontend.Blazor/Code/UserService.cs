using Frontend.Blazor.HttpClients;
using Frontend.Blazor.Models;
using Microsoft.IdentityModel.Tokens;

namespace Frontend.Blazor.Code;

public class UserService(IBackendApiHttpClient backendApiHttpClient)
{
    public async Task<UsersResponse> GetAllUsers(string authToken)
    {
        var response = await backendApiHttpClient.GetAllUsersAsync(authToken);
        if (!response.Errors.IsNullOrEmpty())
        {
            throw new HttpRequestException($"request for users failed: {response.Errors}");
        }

        return response.Result;
    }
}