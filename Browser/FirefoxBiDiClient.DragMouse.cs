using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class FirefoxBiDiClient
{
    public void DragAndDrop(string remoteDebuggingUrl, string sourceSelector, string targetSelector) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext(remoteDebuggingUrl);
            var source = await GetElementRect(session, context.Id, sourceSelector);
            var target = await GetElementRect(session, context.Id, targetSelector);
            await EnsurePointInViewport(session, context.Id, sourceSelector, source.X + source.Width / 2, source.Y + source.Height / 2);
            await EnsurePointInViewport(session, context.Id, targetSelector, target.X + target.Width / 2, target.Y + target.Height / 2);
            _ = ReadScriptResultValue(await Evaluate(session, context.Id, BrowserDomScripts.DragAndDrop(sourceSelector, targetSelector)));
            return true;
        });

    public void MouseDragAndDrop(string remoteDebuggingUrl, string sourceSelector, string targetSelector, IReadOnlyList<ElementPoint> path, Action<ElementPoint>? afterMove = null)
    {
        if (path.Count is 0)
        {
            return;
        }

        MouseDown(remoteDebuggingUrl, path[0]);
        try
        {
            foreach (var point in path)
            {
                MoveMouse(remoteDebuggingUrl, point, buttons: 1);
                afterMove?.Invoke(point);
            }
        }
        finally
        {
            MouseUp(remoteDebuggingUrl, path[^1]);
        }
    }

    public void BeginPageDrag(string remoteDebuggingUrl, string sourceSelector, ElementPoint point) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.BeginDrag(sourceSelector, point));

    public void MovePageDrag(string remoteDebuggingUrl, ElementPoint point) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.MoveDrag(point));

    public void EndPageDrag(string remoteDebuggingUrl, ElementPoint point) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.EndDrag(point));

    public void RemoveDefaultDragGhost(string remoteDebuggingUrl) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.RemoveDefaultDragGhost());

    public void MoveMouse(string remoteDebuggingUrl, ElementPoint point, int buttons) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.MoveMouse(point, buttons));

    public void MouseDown(string remoteDebuggingUrl, ElementPoint point) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.MouseDown(point));

    public void MouseUp(string remoteDebuggingUrl, ElementPoint point) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.MouseUp(point));
}
