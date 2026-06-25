using System.Net.Http.Headers;

namespace CMG.Runner;

public sealed class CmgApiRequestRunner
{
    private static readonly HttpClient SharedClient = new();
    private readonly HttpClient client;

    public CmgApiRequestRunner(HttpClient? client = null)
    {
        this.client = client ?? SharedClient;
    }

    public CmgStepResult Run(CmgNode action)
    {
        if (action.Arguments.Count < 2)
        {
            return Fail(action, "apiRequest requires method and URL arguments.");
        }

        try
        {
            using var request = new HttpRequestMessage(new HttpMethod(action.Arguments[0]), BuildUri(action));
            AddHeaders(request.Headers, action);
            AddContent(request, action);
            using var cancellation = new CancellationTokenSource(GetTimeout(action));
            using var response = client.Send(request, cancellation.Token);
            var responseBody = response.Content.ReadAsStringAsync(cancellation.Token).GetAwaiter().GetResult();
            var output = BuildOutput(action, response, responseBody);
            return Validate(action, response, responseBody, output);
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException or UriFormatException or TaskCanceledException or FormatException)
        {
            return Fail(action, exception is TaskCanceledException ? "API request timed out." : exception.Message);
        }
    }

    private static Uri BuildUri(CmgNode action)
    {
        var builder = new UriBuilder(action.Arguments[1]);
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(builder.Query))
        {
            query.Add(builder.Query.TrimStart('?'));
        }

        query.AddRange(action.Options
            .Where(option => option.Key.StartsWith("query.", StringComparison.OrdinalIgnoreCase))
            .Select(option => $"{Uri.EscapeDataString(option.Key["query.".Length..])}={Uri.EscapeDataString(option.Value)}"));
        builder.Query = string.Join('&', query.Where(part => !string.IsNullOrWhiteSpace(part)));
        return builder.Uri;
    }

    private static void AddContent(HttpRequestMessage request, CmgNode action)
    {
        var content = action.Options.TryGetValue("json", out var json)
            ? new StringContent(json)
            : action.Options.TryGetValue("body", out var body)
                ? new StringContent(body)
                : null;
        if (content is null)
        {
            return;
        }

        if (action.Options.TryGetValue("contentType", out var contentType))
        {
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        }
        else if (action.Options.ContainsKey("json"))
        {
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        }

        request.Content = content;
    }

    private static TimeSpan GetTimeout(CmgNode action)
    {
        if (!action.Options.TryGetValue("timeout", out var value))
        {
            return TimeSpan.FromSeconds(30);
        }

        return int.TryParse(value, out var milliseconds) && milliseconds > 0
            ? TimeSpan.FromMilliseconds(milliseconds)
            : throw new InvalidOperationException("apiRequest option timeout= must be a positive number of milliseconds.");
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
