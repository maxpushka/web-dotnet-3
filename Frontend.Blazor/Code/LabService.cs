using Frontend.Blazor.HttpClients;
using Frontend.Blazor.Models;
using Microsoft.IdentityModel.Tokens;

namespace Frontend.Blazor.Code;

public class LabService(IBackendApiHttpClient backendApiHttpClient)
{
    public async Task<AnalysisResponse> AnalyzeLab(string authToken, AnalysisRequest analysisRequest)
    {
        var response = await backendApiHttpClient.AnalyzeLabAsync(authToken, analysisRequest);
        if (!response.Errors.IsNullOrEmpty())
        {
            throw new HttpRequestException($"lab analysis failed: {response.Errors}");
        }

        return response.Result;
    }

    public async Task<LabsResponse> GetAllLabsAsync(string authToken)
    {
        var response = await backendApiHttpClient.GetAllLabsAsync(authToken);
        if (!response.Errors.IsNullOrEmpty())
        {
            throw new HttpRequestException($"request for labs failed: {response.Errors}");
        }

        return response.Result;
    }
}