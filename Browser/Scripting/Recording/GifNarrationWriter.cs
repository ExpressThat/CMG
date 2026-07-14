using System.Text;

namespace CMG.Browser.Scripting.Recording;

public static class GifNarrationWriter
{
    public static string Write(
        string path,
        string gifPath,
        GifReviewOptions review,
        GifFrameSink sink,
        IReadOnlyList<GifTimelineStep> steps,
        GifRecordingOutcome? outcome = null)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? ".");
        var builder = new StringBuilder();
        builder.AppendLine($"CMG visual evidence: {Path.GetFileName(gifPath)}");
        if (!string.IsNullOrWhiteSpace(review.Description)) builder.AppendLine($"Description: {review.Description}");
        var altText = review.RenderAltText(gifPath, sink, steps, outcome);
        if (!string.IsNullOrWhiteSpace(altText)) builder.AppendLine($"Alt text: {altText}");
        builder.AppendLine($"Duration: {sink.DurationMilliseconds} milliseconds");
        builder.AppendLine($"Outcome: {outcome?.ToString().ToLowerInvariant() ?? (steps.Any(step => !step.Success) ? "failed" : "passed")}");
        builder.AppendLine("Steps:");
        foreach (var step in steps.OrderBy(step => step.Sequence))
        {
            var context = string.IsNullOrWhiteSpace(step.Context) ? string.Empty : $" in {step.Context}";
            var status = step.Success ? "passed" : "failed";
            builder.AppendLine($"{step.Sequence}. {step.Action}{context}, {status}, {step.StartTimeMilliseconds}-{step.EndTimeMilliseconds} ms.");
        }
        File.WriteAllText(fullPath, builder.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return fullPath;
    }
}
