using System.Globalization;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Browser.Scripting.Recording;

public static class GifStillPdfWriter
{
    public static string Write(
        string path,
        string recordingPath,
        GifFrameSink sink,
        IReadOnlyList<GifTimelineStep> steps)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var indices = SelectFrames(sink.FrameCount, steps);
        try
        {
            WriteDocument(fullPath, recordingPath, sink, indices);
            return fullPath;
        }
        catch
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
            throw;
        }
    }

    private static void WriteDocument(
        string fullPath,
        string recordingPath,
        GifFrameSink sink,
        IReadOnlyList<int> indices)
    {
        using var stream = File.Create(fullPath);
        var offsets = new long[4 + indices.Count * 3];
        WriteAscii(stream, "%PDF-1.4\n%CMG\n");
        WriteObject(stream, offsets, 1, "<< /Type /Catalog /Pages 2 0 R >>");
        var kids = string.Join(' ', Enumerable.Range(0, indices.Count).Select(i => $"{4 + i * 3} 0 R"));
        WriteObject(stream, offsets, 2, $"<< /Type /Pages /Count {indices.Count} /Kids [{kids}] >>");
        WriteObject(stream, offsets, 3, $"<< /Title ({Escape(Path.GetFileName(recordingPath))} step review) /Producer (CMG) >>");
        var page = 0;
        sink.VisitCanvases(indices.ToHashSet(), (_, image) => WritePage(stream, offsets, page++, image));
        var xref = stream.Position;
        WriteAscii(stream, $"xref\n0 {offsets.Length}\n0000000000 65535 f \n");
        for (var id = 1; id < offsets.Length; id++)
            WriteAscii(stream, $"{offsets[id]:0000000000} 00000 n \n");
        WriteAscii(stream, $"trailer\n<< /Size {offsets.Length} /Root 1 0 R /Info 3 0 R >>\nstartxref\n{xref}\n%%EOF\n");
    }

    private static IReadOnlyList<int> SelectFrames(int frameCount, IReadOnlyList<GifTimelineStep> steps)
    {
        var selected = steps
            .Select(step => step.FailureFrameIndex ?? step.EndFrameIndex ?? step.StartFrameIndex)
            .Where(index => index >= 0 && index < frameCount)
            .Distinct().ToList();
        if (frameCount > 0 && !selected.Contains(frameCount - 1)) selected.Add(frameCount - 1);
        if (selected.Count is 0 && frameCount > 0) selected.Add(0);
        selected.Sort();
        return selected;
    }

    private static void WritePage(Stream stream, long[] offsets, int index, Image<Rgba32> image)
    {
        var pageId = 4 + index * 3;
        var imageId = pageId + 1;
        var contentId = pageId + 2;
        var width = image.Width.ToString(CultureInfo.InvariantCulture);
        var height = image.Height.ToString(CultureInfo.InvariantCulture);
        WriteObject(stream, offsets, pageId,
            $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {width} {height}] /Resources << /XObject << /Im {imageId} 0 R >> >> /Contents {contentId} 0 R >>");
        using var jpeg = new MemoryStream();
        image.Save(jpeg, new JpegEncoder { Quality = 95 });
        WriteStreamObject(stream, offsets, imageId,
            $"/Type /XObject /Subtype /Image /Width {width} /Height {height} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode",
            jpeg.ToArray());
        WriteStreamObject(stream, offsets, contentId, string.Empty,
            Encoding.ASCII.GetBytes($"q {width} 0 0 {height} 0 0 cm /Im Do Q\n"));
    }

    private static void WriteObject(Stream stream, long[] offsets, int id, string body)
    {
        offsets[id] = stream.Position;
        WriteAscii(stream, $"{id} 0 obj\n{body}\nendobj\n");
    }

    private static void WriteStreamObject(Stream stream, long[] offsets, int id, string dictionary, byte[] body)
    {
        offsets[id] = stream.Position;
        WriteAscii(stream, $"{id} 0 obj\n<< {dictionary} /Length {body.Length} >>\nstream\n");
        stream.Write(body);
        WriteAscii(stream, "\nendstream\nendobj\n");
    }

    private static void WriteAscii(Stream stream, string value) =>
        stream.Write(Encoding.ASCII.GetBytes(value));

    private static string Escape(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
}
