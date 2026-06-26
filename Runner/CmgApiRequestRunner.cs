using System.Net.Http.Headers;
using System.Text;

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
            WriteBodyFile(action, responseBody);
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
        HttpContent? content = FormValues(action) is { Count: > 0 } form
            ? new FormUrlEncodedContent(form)
            : null;
        content ??= action.Options.TryGetValue("json", out var json)
            ? new StringContent(json)
            : action.Options.TryGetValue("body", out var body) ? new StringContent(body) : null;
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

        if (action.Options.TryGetValue("auth", out var auth))
        {
            headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(auth)));
        }
    }

    private static IReadOnlyList<string> BuildOutput(CmgNode action, HttpResponseMessage response, string body)
    {
        var output = new List<string> { $"API {action.LineNumber:000} {(int)response.StatusCode} {response.RequestMessage?.RequestUri}" };
        if (action.Options.TryGetValue("output", out var outputPath) && !string.IsNullOrWhiteSpace(outputPath))
        {
            output.Add($"API_BODY_FILE {action.LineNumber:000} {Path.GetFullPath(outputPath)}");
        }
        else
        {
            output.Add($"API_BODY {action.LineNumber:000} {body}");
        }

        return output;
    }

    private static CmgStepResult Validate(CmgNode action, HttpResponseMessage response, string body, IReadOnlyList<string> output)
    {
        var expectOk = false;
        if (action.Options.TryGetValue("ok", out var ok) && !bool.TryParse(ok, out expectOk))
        {
            return Fail(action, "apiRequest option ok= must be true or false.", output);
        }

        if (action.Options.ContainsKey("ok") && response.IsSuccessStatusCode != expectOk)
        {
            return Fail(action, $"Expected ok={expectOk.ToString().ToLowerInvariant()}, got status {(int)response.StatusCode}.", output);
        }

        if (action.Options.TryGetValue("status", out var expectedStatus) && !StatusMatches(expectedStatus, (int)response.StatusCode))
        {
            return Fail(action, $"Expected status {expectedStatus}, got {(int)response.StatusCode}.", output);
        }

        if (action.Options.TryGetValue("contains", out var expectedText) &&
            !body.Contains(expectedText, StringComparison.Ordinal))
        {
            return Fail(action, $"Expected response body to contain '{expectedText}'.", output);
        }

        if (action.Options.TryGetValue("notContains", out var rejectedText) &&
            body.Contains(rejectedText, StringComparison.Ordinal))
        {
            return Fail(action, $"Expected response body not to contain '{rejectedText}'.", output);
        }

        foreach (var option in action.Options.Where(option => option.Key.StartsWith("expectHeader.", StringComparison.OrdinalIgnoreCase)))
        {
            var name = option.Key["expectHeader.".Length..];
            var actual = HeaderValue(response, name);
            if (actual is null || !actual.Contains(option.Value, StringComparison.OrdinalIgnoreCase))
            {
                return Fail(action, $"Expected response header '{name}' to contain '{option.Value}'.", output);
            }
        }

        return new CmgStepResult(action.LineNumber, action.Kind, true, output, null, null);
    }

    private static Dictionary<string, string> FormValues(CmgNode action) =>
        action.Options
            .Where(option => option.Key.StartsWith("form.", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(option => option.Key["form.".Length..], option => option.Value, StringComparer.OrdinalIgnoreCase);

    private static void WriteBodyFile(CmgNode action, string body)
    {
        if (!action.Options.TryGetValue("output", out var outputPath) || string.IsNullOrWhiteSpace(outputPath))
        {
            return;
        }

        var fullPath = Path.GetFullPath(outputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory());
        File.WriteAllText(fullPath, body);
    }

    private static bool StatusMatches(string expected, int actual)
    {
        return expected.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Any(part => part.Split('-', 2) is var bounds && bounds.Length == 2
                ? int.TryParse(bounds[0], out var min) && int.TryParse(bounds[1], out var max) && actual >= min && actual <= max
                : int.TryParse(part, out var status) && actual == status);
    }

    private static string? HeaderValue(HttpResponseMessage response, string name)
    {
        return response.Headers.TryGetValues(name, out var values) || response.Content.Headers.TryGetValues(name, out values)
            ? string.Join(",", values)
            : null;
    }

    private static CmgStepResult Fail(CmgNode action, string error, IReadOnlyList<string>? output = null) =>
        new(action.LineNumber, action.Kind, false, output ?? [], error, null);
}
