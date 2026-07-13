using System.Net;
using System.Text;

namespace CMG.Runner;

public static partial class CmgHtmlReportWriter
{
    private static void WriteEvidenceLinks(StringBuilder builder, CmgStepResult step, int testIndex)
    {
        builder.Append("<td class=\"evidence-links\">");
        var rendered = 0;
        for (var index = 0; index < step.GifEvidence.Count; index++)
        {
            var evidence = step.GifEvidence[index];
            if (evidence.EndFrameIndex is null)
            {
                continue;
            }

            if (rendered++ > 0)
            {
                builder.Append("<br>");
            }

            builder.Append($"<a href=\"#evidence-{testIndex}-{step.Sequence}-{index}\">frame {evidence.StartFrameIndex}</a>");
            if (evidence.FailureFrameIndex is int failure && failure != evidence.StartFrameIndex)
            {
                builder.Append($" / <a href=\"#failure-{testIndex}\">failure {failure}</a>");
            }
        }
        builder.AppendLine("</td>");
    }

    private static void WriteFailureFrame(StringBuilder builder, CmgTestResult test, int testIndex)
    {
        var evidence = test.Steps
            .SelectMany(step => step.GifEvidence)
            .FirstOrDefault(item => item.FailureFrameIndex is not null);
        if (evidence?.FailureFrameIndex is not int frameIndex)
        {
            return;
        }

        var source = CmgGifFrameRenderer.DataUri(evidence.GifPath, frameIndex);
        if (source is null)
        {
            return;
        }

        builder.AppendLine($"<figure class=\"evidence-frame failure-frame\" id=\"failure-{testIndex}\">");
        builder.AppendLine($"<img src=\"{source}\" alt=\"Failure frame {frameIndex}\">");
        builder.AppendLine($"<figcaption>Failure frame {frameIndex}: {WebUtility.HtmlEncode(evidence.GifPath)}</figcaption>");
        builder.AppendLine("</figure>");
    }

    private static void WriteEvidenceGallery(
        StringBuilder builder,
        IReadOnlyList<CmgStepResult> steps,
        int testIndex)
    {
        var rendered = false;
        foreach (var step in steps)
        {
            for (var index = 0; index < step.GifEvidence.Count; index++)
            {
                var evidence = step.GifEvidence[index];
                if (evidence.EndFrameIndex is null)
                {
                    continue;
                }

                var source = CmgGifFrameRenderer.DataUri(evidence.GifPath, evidence.StartFrameIndex);
                if (source is null)
                {
                    continue;
                }

                if (!rendered)
                {
                    builder.AppendLine("<h3>Step Evidence</h3><div class=\"gif-evidence\">");
                    rendered = true;
                }

                var action = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(step.Action) ? step.Name : step.Action);
                builder.AppendLine($"<figure class=\"evidence-frame\" id=\"evidence-{testIndex}-{step.Sequence}-{index}\">");
                builder.AppendLine($"<img src=\"{source}\" alt=\"Start frame for {action}\">");
                builder.AppendLine($"<figcaption>Step {step.Sequence}, {action}: frame {evidence.StartFrameIndex} at {evidence.StartTimeMilliseconds}ms</figcaption>");
                builder.AppendLine("</figure>");
            }
        }

        if (rendered)
        {
            builder.AppendLine("</div>");
        }
    }
}
