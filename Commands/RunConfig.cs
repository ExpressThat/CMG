using System.Text.Json;

namespace CMG.Commands;

internal sealed record RunConfig(
    DirectoryInfo? Gif,
    FileInfo? ReportJson,
    FileInfo? ReportHtml,
    FileInfo? ReportJunit,
    DirectoryInfo? Trace,
    string? Grep,
    string? Tag,
    int? Retries,
    int? MaxFailures,
    int? RepeatEach,
    string? Shard,
    int? Timeout,
    int? NavigationTimeout,
    int? AssertionTimeout,
    string? BaseUrl,
    IReadOnlyDictionary<string, string> Variables)
{
    public static RunConfig Empty { get; } = new(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, new Dictionary<string, string>());
}

internal static class RunConfigReader
{
    public static bool TryRead(FileInfo? file, out RunConfig config, out string? error)
    {
        config = RunConfig.Empty;
        error = null;
        if (file is null)
        {
            return true;
        }
        if (!file.Exists)
        {
            error = $"Run config '{file.FullName}' did not exist.";
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(file.FullName), new JsonDocumentOptions { AllowTrailingCommas = true });
            if (document.RootElement.ValueKind is not JsonValueKind.Object)
            {
                error = "Run config root must be a JSON object.";
                return false;
            }

            config = ReadObject(document.RootElement, file.DirectoryName ?? Directory.GetCurrentDirectory());
            return true;
        }
        catch (JsonException exception)
        {
            error = $"Run config '{file.FullName}' was not valid JSON. {exception.Message}";
            return false;
        }
        catch (RunConfigException exception)
        {
            error = exception.Message;
            return false;
        }
    }

    private static RunConfig ReadObject(JsonElement root, string baseDirectory) => new(
        DirectoryOption(root, "gif", baseDirectory),
        FileOption(root, "reportJson", baseDirectory),
        FileOption(root, "reportHtml", baseDirectory),
        FileOption(root, "reportJunit", baseDirectory),
        DirectoryOption(root, "trace", baseDirectory),
        StringOption(root, "grep"),
        StringOption(root, "tag"),
        IntOption(root, "retries"),
        IntOption(root, "maxFailures"),
        IntOption(root, "repeatEach"),
        StringOption(root, "shard"),
        IntOption(root, "timeout"),
        IntOption(root, "navigationTimeout"),
        IntOption(root, "assertionTimeout"),
        StringOption(root, "baseUrl"),
        Variables(root));

    private static FileInfo? FileOption(JsonElement root, string name, string baseDirectory) =>
        StringOption(root, name) is { } value ? new FileInfo(ResolvePath(baseDirectory, value)) : null;

    private static DirectoryInfo? DirectoryOption(JsonElement root, string name, string baseDirectory) =>
        StringOption(root, name) is { } value ? new DirectoryInfo(ResolvePath(baseDirectory, value)) : null;

    private static string? StringOption(JsonElement root, string name) =>
        !root.TryGetProperty(name, out var value)
            ? null
            : value.ValueKind is JsonValueKind.String
                ? value.GetString()
                : throw new RunConfigException($"Run config option '{name}' must be a string.");

    private static int? IntOption(JsonElement root, string name) =>
        !root.TryGetProperty(name, out var value)
            ? null
            : value.ValueKind is JsonValueKind.Number && value.TryGetInt32(out var number)
                ? number
                : throw new RunConfigException($"Run config option '{name}' must be an integer.");

    private static IReadOnlyDictionary<string, string> Variables(JsonElement root)
    {
        if (!root.TryGetProperty("variables", out var variables) || variables.ValueKind is not JsonValueKind.Object)
        {
            if (root.TryGetProperty("variables", out variables))
            {
                throw new RunConfigException("Run config option 'variables' must be an object.");
            }

            return new Dictionary<string, string>();
        }

        return variables.EnumerateObject()
            .ToDictionary(property => property.Name, property => property.Value.ToString(), StringComparer.OrdinalIgnoreCase);
    }

    private static string ResolvePath(string baseDirectory, string value) =>
        Path.IsPathRooted(value) ? value : Path.GetFullPath(Path.Combine(baseDirectory, value));

    private sealed class RunConfigException : Exception
    {
        public RunConfigException(string message) : base(message) { }
    }
}
