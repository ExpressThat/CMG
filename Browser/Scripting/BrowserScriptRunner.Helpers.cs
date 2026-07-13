using System.Text.RegularExpressions;
using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static BrowserScriptAction ExpandVariables(BrowserScriptAction action, ScriptExecutionContext context)
    {
        return action with
        {
            Arguments = action.Arguments.Select(argument => ExpandVariables(argument, context)).ToArray(),
            Options = action.Options.ToDictionary(
                pair => pair.Key,
                pair => ExpandVariables(pair.Value, context),
                StringComparer.OrdinalIgnoreCase)
        };
    }

    private static string ExpandVariables(string value, ScriptExecutionContext context)
    {
        return VariableRegex().Replace(value, match =>
        {
            var name = match.Groups[1].Value;
            if (!context.TryGetVariable(name, out var replacement))
            {
                throw new ScriptExecutionException($"Variable '{name}' is not defined.");
            }

            return replacement;
        });
    }

    private static ScriptReadResult ReadScript(string file)
    {
        if (string.IsNullOrWhiteSpace(file))
        {
            return ScriptReadResult.Fail("Script file was not provided.");
        }

        if (file is "-")
        {
            var script = Console.In.ReadToEnd();
            return string.IsNullOrEmpty(script)
                ? ScriptReadResult.Fail("No script text was provided on stdin for --file -.")
                : ScriptReadResult.Ok(script);
        }

        if (!File.Exists(file))
        {
            return ScriptReadResult.Fail($"Script file '{file}' was not found.");
        }

        var fullPath = Path.GetFullPath(file);
        var expanded = ScriptImportExpander.Expand(
            File.ReadAllText(fullPath),
            Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory());
        return expanded.Success
            ? ScriptReadResult.Ok(expanded.Script ?? string.Empty)
            : ScriptReadResult.Fail(expanded.Error ?? "Could not import script.");
    }

    private static string NormalizeNavigationTarget(string target, string? baseUrl = null)
    {
        if (IsAbsoluteUri(target))
        {
            return target;
        }

        if (Path.IsPathRooted(target) && File.Exists(target))
        {
            return new Uri(Path.GetFullPath(target)).AbsoluteUri;
        }

        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            return new Uri(new Uri(baseUrl), target).AbsoluteUri;
        }

        if (File.Exists(target))
        {
            return new Uri(Path.GetFullPath(target)).AbsoluteUri;
        }

        if (LooksLikeLocalPath(target))
        {
            throw new ScriptExecutionException($"Navigation target path '{target}' was not found.");
        }

        return target;
    }

    private static string? NormalizeBaseUrl(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return null;
        }

        return Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)
            ? uri.AbsoluteUri
            : throw new ScriptExecutionException($"baseUrl must be an absolute URL, got '{baseUrl}'.");
    }

    private static bool IsAbsoluteUri(string target) =>
        Uri.TryCreate(target, UriKind.Absolute, out _);

    private static bool LooksLikeLocalPath(string target) =>
        !target.Contains("://", StringComparison.Ordinal) &&
        (Path.IsPathRooted(target) ||
        target.StartsWith(".", StringComparison.Ordinal) ||
        target.Contains(Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
        target.Contains(Path.AltDirectorySeparatorChar, StringComparison.Ordinal));

    private static void RequireArgumentCount(BrowserScriptAction action, int min, int max)
    {
        if (action.Arguments.Count < min || action.Arguments.Count > max)
        {
            var expected = min == max ? min.ToString() : $"{min}-{max}";
            throw new ScriptExecutionException($"Expected {expected} positional argument(s), got {action.Arguments.Count}.");
        }
    }

    private static int GetIntOption(BrowserScriptAction action, string name, int defaultValue)
    {
        return action.Options.TryGetValue(name, out var value)
            ? ParsePositiveInt(value, name)
            : defaultValue;
    }

    private static int GetIntOption(BrowserScriptAction action, string name, bool required)
    {
        if (!action.Options.TryGetValue(name, out var value))
        {
            if (required)
            {
                throw new ScriptExecutionException($"Missing required option '{name}'.");
            }

            return 0;
        }

        return ParsePositiveInt(value, name);
    }

    private static int ParsePositiveInt(string value, string name)
    {
        if (!int.TryParse(value, out var number) || number < 0)
        {
            throw new ScriptExecutionException($"{name}= must be zero or greater.");
        }

        return number;
    }

    private static string FormatActionForLog(BrowserScriptAction action)
    {
        return string.Join(' ', action.Arguments.Select(QuoteForLog));
    }

    private static string QuoteForLog(string value)
    {
        return value.Contains(' ', StringComparison.Ordinal) ? $"\"{value}\"" : value;
    }

    private static void FinishRecording(ScriptGifRecorder? recorder, List<string> output, bool failure = false)
    {
        if (recorder is null)
        {
            return;
        }

        if (failure)
        {
            recorder.CaptureFailureHold();
        }

        recorder.Finish();
        output.Add($"GIF {recorder.OutputPath}");
        if (recorder.RetainedFramesDirectory is not null)
        {
            var path = $"\"{System.Text.Json.JsonEncodedText.Encode(Path.GetFullPath(recorder.RetainedFramesDirectory))}\"";
            output.Add($"GIF_FRAMES path={path} count={recorder.FrameCount}");
        }
        if (!string.IsNullOrWhiteSpace(recorder.TimelinePath))
        {
            output.Add($"GIF_TIMELINE {recorder.TimelinePath}");
        }
        if (!string.IsNullOrWhiteSpace(recorder.DebugPath))
        {
            output.Add($"GIF_DEBUG {recorder.DebugPath}");
        }
    }

    private static void FinishTrace(ScriptExecutionContext context, bool success, string? error, List<string> output)
    {
        if (context.Trace?.IsActive is not true || context.Trace.OutputPath is null)
        {
            return;
        }

        var path = context.Trace.Finish(null, success, error);
        output.Add($"TRACE {path}");
    }

    [GeneratedRegex(@"\$\{([A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*)\}")]
    private static partial Regex VariableRegex();
}
