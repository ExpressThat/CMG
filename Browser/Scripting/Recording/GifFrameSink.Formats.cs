using System.ComponentModel;
using System.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Browser.Scripting.Recording;

public sealed partial class GifFrameSink
{
    private void EncodeArtifact(Image<Rgba32> image, string path, GifQuality selectedQuality)
    {
        switch (encoding.Format)
        {
            case GifArtifactFormat.Apng:
                SetPngDelays(image);
                image.SaveAsPng(path, new PngEncoder());
                break;
            case GifArtifactFormat.Webp:
                SetWebpDelays(image);
                image.SaveAsWebp(path, WebpEncoderFor(selectedQuality));
                break;
            case GifArtifactFormat.Mp4:
                EncodeMp4(image, path, selectedQuality);
                break;
            default:
                image.SaveAsGif(path, CreateEncoder(selectedQuality, encoding));
                break;
        }
    }

    private static void SetAnimationMetadata(Image<Rgba32> image, GifArtifactFormat format)
    {
        image.Metadata.GetGifMetadata().RepeatCount = 0;
        if (format is GifArtifactFormat.Apng)
        {
            image.Metadata.GetPngMetadata().RepeatCount = 0;
            image.Metadata.GetPngMetadata().AnimateRootFrame = true;
        }
        if (format is GifArtifactFormat.Webp) image.Metadata.GetWebpMetadata().RepeatCount = 0;
    }

    private static void SetPngDelays(Image<Rgba32> image)
    {
        foreach (var frame in image.Frames)
        {
            var delay = frame.Metadata.GetGifMetadata().FrameDelay;
            frame.Metadata.GetPngMetadata().FrameDelay = new Rational((uint)Math.Max(1, delay), 100);
        }
    }

    private static void SetWebpDelays(Image<Rgba32> image)
    {
        foreach (var frame in image.Frames)
        {
            var metadata = frame.Metadata.GetWebpMetadata();
            metadata.FrameDelay = (uint)Math.Max(10, frame.Metadata.GetGifMetadata().FrameDelay * 10);
            metadata.BlendMethod = WebpBlendMethod.Over;
            metadata.DisposalMethod = WebpDisposalMethod.DoNotDispose;
        }
    }

    private static WebpEncoder WebpEncoderFor(GifQuality quality) => new()
    {
        FileFormat = quality is GifQuality.Archival or GifQuality.Highest
            ? WebpFileFormatType.Lossless
            : WebpFileFormatType.Lossy,
        Quality = quality switch
        {
            GifQuality.Low => 55,
            GifQuality.Medium => 70,
            GifQuality.High => 85,
            _ => 100
        }
    };

    private void EncodeMp4(Image<Rgba32> image, string path, GifQuality quality)
    {
        var directory = Path.Combine(Path.GetTempPath(), $"cmg-mp4-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var manifest = WriteMp4Frames(image, directory);
            RunFfmpeg(manifest, path, quality);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    private static string WriteMp4Frames(Image<Rgba32> image, string directory)
    {
        var manifest = Path.Combine(directory, "frames.txt");
        using var writer = new StreamWriter(manifest, append: false, new System.Text.UTF8Encoding(false));
        writer.WriteLine("ffconcat version 1.0");
        for (var index = 0; index < image.Frames.Count; index++)
        {
            var path = Path.Combine(directory, $"frame-{index + 1:0000}.png");
            using var clone = image.Clone();
            for (var remove = clone.Frames.Count - 1; remove >= 0; remove--)
                if (remove != index) clone.Frames.RemoveFrame(remove);
            clone.SaveAsPng(path);
            var quoted = $"file '{path.Replace("'", "'\\''", StringComparison.Ordinal)}'";
            var repeats = Math.Max(1, image.Frames[index].Metadata.GetGifMetadata().FrameDelay);
            for (var repeat = 0; repeat < repeats; repeat++)
            {
                writer.WriteLine(quoted);
                writer.WriteLine("option framerate 100");
            }
        }
        return manifest;
    }

    private void RunFfmpeg(string manifest, string path, GifQuality quality)
    {
        var executable = encoding.FfmpegPath ?? Environment.GetEnvironmentVariable("CMG_FFMPEG") ?? "ffmpeg";
        var start = new ProcessStartInfo(executable) { UseShellExecute = false, RedirectStandardError = true };
        foreach (var argument in new[] { "-hide_banner", "-loglevel", "error", "-f", "concat", "-safe", "0", "-i", manifest,
                     "-an", "-vf", "pad=ceil(iw/2)*2:ceil(ih/2)*2", "-c:v", "libx264", "-preset", "medium", "-crf", Crf(quality), "-bf", "0", "-pix_fmt", "yuv420p",
                     "-enc_time_base", "1:100", "-fps_mode", "passthrough",
                     "-video_track_timescale", "100", "-movflags", "+faststart", "-f", "mp4", "-y", path }) start.ArgumentList.Add(argument);
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
                $"MP4 recording requires FFmpeg. Install ffmpeg, set CMG_FFMPEG, or use ffmpeg=<path>. {exception.Message}");
        }
    }

    private static string Crf(GifQuality quality) => quality switch
    {
        GifQuality.Low => "32", GifQuality.Medium => "27", GifQuality.High => "22", _ => "18"
    };

    private static string Bound(string value) => value.Trim().Length <= 500 ? value.Trim() : value.Trim()[..500];
}
