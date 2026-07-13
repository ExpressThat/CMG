namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    public bool IsCaptureSuspended => captureSuspensionDepth > 0;

    public void SuspendCapture(Action body)
    {
        captureSuspensionDepth++;
        if (captureSuspensionDepth == 1 && remoteDebuggingUrl is not null)
        {
            devToolsClient.RemoveDomCursor(remoteDebuggingUrl);
        }

        try
        {
            body();
        }
        finally
        {
            captureSuspensionDepth--;
        }
    }

    public void WithPlaybackRate(double rate, Action body)
    {
        var previous = playbackRate;
        playbackRate *= rate;
        try
        {
            body();
        }
        finally
        {
            playbackRate = previous;
        }
    }

    private int ScaleDelay(int delayCentiseconds) =>
        Math.Max(1, (int)Math.Round(delayCentiseconds / playbackRate, MidpointRounding.AwayFromZero));
}
