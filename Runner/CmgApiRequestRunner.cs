using System.Net.Http.Headers;

namespace CMG.Runner;

public sealed class CmgApiRequestRunner
{
    private static readonly HttpClient Client = new();

    public CmgStepResult Run(CmgNode action)
    {
        if (action.Arguments.Count < 2)
        {
            return Fail(action, "apiRequest requires method and URL arguments.");
        }

        try
        {
            using var request = new HttpRequestMessage(new HttpMethod(action.Arguments[0]), action.Arguments[1]);
            AddHeaders(request.Headers, action);
            if (action.Options.TryGetValue("body", out var body))
            {
                request.Content = new StringContent(body);
            }

            using var response = Client.Send(request);
            var responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var output = BuildOutput(action, response, responseBody);
            return Validate(action, response, responseBody, output);
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException or UriFormatException)
        {
            return Fail(action, exception.Message);
        }
    }

    private static void AddHeaders(HttpRequestHeaders headers, CmgNode action)
    {
        foreach (var option in action.Options.Where(option => option.Key.StartsWith("header.", StringComparison.OrdinalIgnoreCase)))
        {
            headers.TryAddWithoutValidation(option.Key["header.".Length..], option.Value);
        }
    }

    private static IReadOnlyList<string> BuildOutput(CmgNode action, HttpResponseMessage response, string body) =>
        [
            $"API {action.LineNumber:000} {(int)response.StatusCode} {response.RequestMessage?.RequestUri}",
            $"API_BODY {action.LineNumber:000} {body}"
        ];

    private static CmgStepResult Validate(CmgNode action, HttpResponseMessage response, string body, IReadOnlyList<string> output)
    {
        if (action.Options.TryGetValue("status", out var expectedStatus) &&
            int.TryParse(expectedStatus, out var status) &&
            (int)response.StatusCode != status)
        {
            return Fail(action, $"Expected status {status}, got {(int)response.StatusCode}.", output);
        }

        if (action.Options.TryGetValue("contains", out var expectedText) &&
            !body.Contains(expectedText, StringComparison.Ordinal))
        {
            return Fail(action, $"Expected response body to contain '{expectedText}'.", output);
        }

        return new CmgStepResult(action.LineNumber, action.Kind, true, output, null, null);
    }

    private static CmgStepResult Fail(CmgNode action, string error, IReadOnlyList<string>? output = null) =>
        new(action.LineNumber, action.Kind, false, output ?? [], error, null);
}
