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
        new Option<string?>("--gif-retention") { Description = "Whole-test GIF retention: always, onFailure, onRetry, or off." },
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
        if (!CmgGifRetentionPolicy.TryParseMode(modeValue, out var mode))
        {
            Console.Error.WriteLine(WasProvided(parseResult, options.Mode)
                ? "--gif-retention must be always, onFailure, onRetry, or off."
                : "Run config option 'gifRetention' must be always, onFailure, onRetry, or off.");
            return false;
        }
        if (failure) mode = CmgGifRetentionMode.OnFailure;
        if (retry) mode = CmgGifRetentionMode.OnRetry;

        var sampleRate = IntValue(parseResult, options.SampleRate, config.GifSampleRate);
        if (sampleRate < 1)
        {
            Console.Error.WriteLine("--gif-sample-rate must be at least 1.");
            return false;
        }

        var cleanPassed = BoolValue(parseResult, options.CleanPassed, config.GifCleanPassed);
        policy = new(mode, sampleRate, cleanPassed);
        return true;
    }
}
