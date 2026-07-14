namespace CMG.Browser.Scripting.Recording;

internal sealed class GifStoredFrame(
    string path, int x, int y, int width, int height, int canvasWidth, int canvasHeight, int delay)
{
    public string Path { get; } = path;
    public int X { get; } = x;
    public int Y { get; } = y;
    public int Width { get; } = width;
    public int Height { get; } = height;
    public int CanvasWidth { get; } = canvasWidth;
    public int CanvasHeight { get; } = canvasHeight;
    public int Delay { get; set; } = delay;
    public bool IsDelta => X != 0 || Y != 0 || Width != CanvasWidth || Height != CanvasHeight;
    public long PixelBytes => (long)Width * Height * 4;
}
