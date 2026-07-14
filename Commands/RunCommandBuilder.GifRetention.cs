using System.CommandLine;
using CMG.Runner;

namespace CMG.Commands;

public sealed partial class RunCommandBuilder
{
    private sealed record GifRetentionCliOptions(
        Option<string?> Mode,
        Option<bool> OnFailure,
        Option<bool> OnRetry,
        Option<int> SampleRate,
        Option<bool> CleanPassed);

    private static GifRetentionCliOptions BuildGifRetentionOptions() => new(
        new Option<string?>("--gif-retention") { Description = "Whole-test GIF retention: all, failed, none, onRetry, or days:<n>." },
        new Option<bool>("--gif-on-failure") { Description = "Keep whole-test GIFs only when the final test result fails." },
        new Option<bool>("--gif-on-retry") { Description = "Keep failed-attempt GIFs and remove the passing attempt GIF." },
        new Option<int>("--gif-sample-rate") { Description = "Record the first selected test and every nth test.", DefaultValueFactory = _ => 1 },
        new Option<bool>("--gif-clean-passed") { Description = "Delete passing whole-test GIFs after reports and traces are written." });

    private static bool TryParseGifRetention(
        ParseResult parseResult,
        GifRetentionCliOptions options,
        RunConfig config,
        out CmgGifRetentionPolicy policy)
    {
        policy = new(CmgGifRetentionMode.Always, 1, false);
        var failure = parseResult.GetValue(options.OnFailure);
        var retry = parseResult.GetValue(options.OnRetry);
        if ((failure && retry) || (WasProvided(parseResult, options.Mode) && (failure || retry)))
        {
            Console.Error.WriteLine("Use only one of --gif-retention, --gif-on-failure, or --gif-on-retry.");
            return false;
        }

        var modeValue = StringValue(parseResult, options.Mode, config.GifRetention) ?? "always";
        var cleanupDays = ParseCleanupDays(modeValue);
        var mode = CmgGifRetentionMode.Always;
        if (cleanupDays is < 1)
        {
            Console.Error.WriteLine("--gif-retention days:<n> requires a positive integer day count.");
            return false;
        }
        if (cleanupDays is null && !CmgGifRetentionPolicy.TryParseMode(modeValue, out mode))
        {
            Console.Error.WriteLine(WasProvided(parseResult, options.Mode)
                ? "--gif-retention must be all, failed, none, onRetry, or days:<positive-integer>."
                : "Run config option 'gifRetention' must be all, failed, none, onRetry, or days:<positive-integer>.");
            return false;
        }
        else if (cleanupDays is not null) mode = CmgGifRetentionMode.Always;
        if (failure) mode = CmgGifRetentionMode.OnFailure;
        if (retry) mode = CmgGifRetentionMode.OnRetry;

        var sampleRate = IntValue(parseResult, options.SampleRate, config.GifSampleRate);
        if (sampleRate < 1)
        {
            Console.Error.WriteLine("--gif-sample-rate must be at least 1.");
            return false;
        }

        var cleanPassed = BoolValue(parseResult, options.CleanPassed, config.GifCleanPassed);
        policy = new(mode, sampleRate, cleanPassed, cleanupDays);
        return true;
    }

    private static int? ParseCleanupDays(string value)
    {
        if (!value.StartsWith("days:", StringComparison.OrdinalIgnoreCase)) return null;
        return int.TryParse(value[5..], out var days) && days > 0 ? days : -1;
    }

    private static bool TryCleanExpiredGifArtifacts(bool listOnly, DirectoryInfo? directory, int? days)
    {
        if (listOnly || directory is null || days is null) return true;
        try
        {
            foreach (var line in GifArtifactRetentionCleaner.Clean(directory, days.Value)) Console.WriteLine(line);
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"GIF retention cleanup failed for '{directory.FullName}'. {exception.Message}");
            return false;
        }
    }
}
