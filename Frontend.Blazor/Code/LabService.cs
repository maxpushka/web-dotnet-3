using Frontend.Blazor.HttpClients;
using Frontend.Blazor.Models;

namespace Frontend.Blazor.Code;

public class LabService(IBackendApiHttpClient backendApiHttpClient)
{
    public async Task<AnalysisResponse> AnalyzeLab(string authToken, AnalysisRequest analysisRequest)
    {
        var response = await backendApiHttpClient.AnalyzeLabAsync(authToken, analysisRequest);
        if (response.Result is null)
        {
            throw new HttpRequestException($"lab analysis failed: {response.Errors}");
        }

        return response.Result;
    }
}