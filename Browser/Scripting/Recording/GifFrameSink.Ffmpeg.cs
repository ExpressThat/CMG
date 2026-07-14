using System.ComponentModel;
using System.Diagnostics;

namespace CMG.Browser.Scripting.Recording;

public sealed partial class GifFrameSink
{
    private void RunFfmpeg(string manifest, string path, GifQuality quality)
    {
        var executable = FfmpegExecutableResolver.Resolve(encoding.FfmpegPath);
        var start = new ProcessStartInfo(executable.Path)
        {
            UseShellExecute = false,
            RedirectStandardError = true
        };
        foreach (var argument in CommonArguments(manifest)) start.ArgumentList.Add(argument);
        foreach (var argument in EncoderArguments(executable.IsBundled, quality)) start.ArgumentList.Add(argument);
        foreach (var argument in OutputArguments(path)) start.ArgumentList.Add(argument);

        try
        {
            using var process = Process.Start(start) ?? throw new InvalidOperationException("FFmpeg did not start.");
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0)
                throw new CMG.Browser.Scripting.ScriptExecutionException(
                    $"MP4 encoding failed with FFmpeg exit code {process.ExitCode}: {Bound(error)}");
        }
        catch (Win32Exception exception)
        {
            throw new CMG.Browser.Scripting.ScriptExecutionException(
                "MP4 recording requires FFmpeg. Use the official CMG release with bundled FFmpeg, " +
                $"set CMG_FFMPEG, add ffmpeg to PATH, or use ffmpeg=<path>. {exception.Message}");
        }
    }

    private static string[] CommonArguments(string manifest) =>
    [
        "-hide_banner", "-loglevel", "error", "-f", "concat", "-safe", "0", "-i", manifest,
        "-an", "-vf", "pad=ceil(iw/2)*2:ceil(ih/2)*2"
    ];

    private static string[] EncoderArguments(bool bundled, GifQuality quality) => bundled
        ? ["-c:v", "libopenh264", "-b:v", OpenH264Bitrate(quality), "-bf", "0"]
        : ["-c:v", "libx264", "-preset", "medium", "-crf", Crf(quality), "-bf", "0"];

    private static string[] OutputArguments(string path) =>
    [
        "-pix_fmt", "yuv420p", "-enc_time_base", "1:100", "-fps_mode", "passthrough",
        "-video_track_timescale", "100", "-movflags", "+faststart", "-f", "mp4", "-y", path
    ];

    private static string Crf(GifQuality quality) => quality switch
    {
        GifQuality.Low => "32", GifQuality.Medium => "27", GifQuality.High => "22", _ => "18"
    };

    private static string OpenH264Bitrate(GifQuality quality) => quality switch
    {
        GifQuality.Low => "750k",
        GifQuality.Medium => "1500k",
        GifQuality.High => "3000k",
        GifQuality.Archival => "10000k",
        _ => "6000k"
    };

    private static string Bound(string value) => value.Trim().Length <= 500 ? value.Trim() : value.Trim()[..500];
}
