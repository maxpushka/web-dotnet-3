using Frontend.Blazor.Models;

namespace Frontend.Blazor.HttpClients;

public class BackendApiHttpClient(HttpClient httpClient) : IBackendApiHttpClient
{
    public async Task<ApiResponse<string>> RegisterUserAsync(UserRegisterInput model,
        CancellationToken? cancellationToken = null)
    {
        return await ApiResponse<string>.HandleExceptionAsync(async () =>
        {
            var response =
                await httpClient.PostAsJsonAsync("api/account", model, cancellationToken ?? CancellationToken.None);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ApiResponse<string>>(cancellationToken ??
                CancellationToken.None);
        });
    }

    public async Task<ApiResponse<AuthResponse>> LoginUserAsync(LoginModel model,
        CancellationToken? cancellationToken = null)
    {
        return await ApiResponse<AuthResponse>.HandleExceptionAsync(async () =>
        {
            var response = await httpClient.PostAsJsonAsync("api/account/login", model);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(cancellationToken ??
                CancellationToken.None);
        });
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshToken,
        CancellationToken? cancellationToken = null)
    {
        return await ApiResponse<AuthResponse>.HandleExceptionAsync(async () =>
        {
            var response = await httpClient.PostAsJsonAsync("api/account/refresh", new { refreshToken });

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(cancellationToken ??
                CancellationToken.None);
        });
    }

    public async Task<ApiResponse<AnalysisResponse>> AnalyzeLabAsync(string authToken, AnalysisRequest analysisRequest,
        CancellationToken? cancellationToken = null)
    {
        return await ApiResponse<AnalysisResponse>.HandleExceptionAsync(async () =>
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            var response = await httpClient.PostAsJsonAsync("api/Analyze/New", analysisRequest);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ApiResponse<AnalysisResponse>>(cancellationToken ??
                CancellationToken.None);
        });
    }
}