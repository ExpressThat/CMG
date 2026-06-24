using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed class ChromeDevToolsClient
{
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    public string GetElementHtml(string remoteDebuggingUrl, string selector)
    {
        return Run(async () =>
        {
            var pageTargets = await GetPageWebSocketDebuggerUrls(remoteDebuggingUrl);

            foreach (var pageTarget in pageTargets)
            {
                await using var session = await DevToolsSession.Connect(pageTarget);

                await session.ScrollElementIntoView(selector);

                var response = await session.SendCommand("Runtime.evaluate", writer =>
                {
                    writer.WriteString("expression", BuildOuterHtmlExpression(selector));
                    writer.WriteBoolean("returnByValue", true);
                });

                if (TryReadString(response, ["result", "result", "value"], out var html) && html is not null)
                {
                    return html;
                }
            }

            throw new ElementNotFoundException(selector);
        });
    }

    public byte[] GetElementScreenshot(string remoteDebuggingUrl, string selector)
    {
        return Run(async () =>
        {
            var pageTargets = await GetPageWebSocketDebuggerUrls(remoteDebuggingUrl);

            foreach (var pageTarget in pageTargets)
            {
                await using var session = await DevToolsSession.Connect(pageTarget);

                await session.ScrollElementIntoView(selector);
                await PromoteMessageBar(session);
                var clip = await GetElementPageClip(session, selector);
                if (clip.Width <= 0 || clip.Height <= 0)
                {
                    throw new ChromeDevToolsException($"Element '{selector}' has no visible area to screenshot.");
                }

                var screenshot = await session.SendCommand("Page.captureScreenshot", writer =>
                {
                    writer.WriteString("format", "png");
                    writer.WriteStartObject("clip");
                    writer.WriteNumber("x", clip.X);
                    writer.WriteNumber("y", clip.Y);
                    writer.WriteNumber("width", clip.Width);
                    writer.WriteNumber("height", clip.Height);
                    writer.WriteNumber("scale", 1);
                    writer.WriteEndObject();
                });

                if (!TryReadString(screenshot, "result", "data", out var data) || string.IsNullOrWhiteSpace(data))
                {
                    throw new ChromeDevToolsException("Chrome did not return screenshot image data.");
                }

                return Convert.FromBase64String(data);
            }

            throw new ElementNotFoundException(selector);
        });
    }

    private static async Task<ElementClip> GetElementPageClip(DevToolsSession session, string selector)
    {
        var response = await session.SendCommand("Runtime.evaluate", writer =>
        {
            writer.WriteString(
                "expression",
                $$"""
                (() => {
                  const element = document.querySelector({{ToJsonStringLiteral(selector)}});
                  if (!element) return null;
                  const rect = element.getBoundingClientRect();
                  return {
                    x: Math.max(0, rect.left + window.scrollX),
                    y: Math.max(0, rect.top + window.scrollY),
                    width: rect.width,
                    height: rect.height
                  };
                })()
                """);
            writer.WriteBoolean("returnByValue", true);
        });

        if (!TryReadElement(response, ["result", "result", "value"], out var value) || value.ValueKind is JsonValueKind.Null)
        {
            throw new ElementNotFoundException(selector);
        }

        if (!TryReadDouble(value, "x", out var x) ||
            !TryReadDouble(value, "y", out var y) ||
            !TryReadDouble(value, "width", out var width) ||
            !TryReadDouble(value, "height", out var height))
        {
            throw new ChromeDevToolsException($"Chrome did not return a screenshot clip for element '{selector}'.");
        }

        return new ElementClip(x, y, width, height);
    }

    public void Navigate(string remoteDebuggingUrl, string target)
    {
        Run(async () =>
        {
            await using var session = await OpenPrimaryPageSession(remoteDebuggingUrl);
            await session.SendCommand("Page.navigate", writer => writer.WriteString("url", target));
            await Task.Delay(500);

            return true;
        });
    }

    public void WaitForElement(string remoteDebuggingUrl, string selector, int timeoutMilliseconds)
    {
        Run(async () =>
        {
            var deadline = DateTimeOffset.UtcNow.AddMilliseconds(timeoutMilliseconds);

            while (DateTimeOffset.UtcNow <= deadline)
            {
                if (await TryFindPageWithSelector(remoteDebuggingUrl, selector) is not null)
                {
                    return true;
                }

                await Task.Delay(PollInterval);
            }

            throw new ElementNotFoundException(selector);
        });
    }

    public void Click(string remoteDebuggingUrl, string selector)
    {
        Run(async () =>
        {
            var pageTarget = await TryFindPageWithSelector(remoteDebuggingUrl, selector) ??
                throw new ElementNotFoundException(selector);

            await using var session = await DevToolsSession.Connect(pageTarget);
            await session.ScrollElementIntoView(selector);

            var clip = await GetElementClip(session, selector);
            await ClickAt(session, clip.CenterX, clip.CenterY);

            return true;
        });
    }

    public void Type(string remoteDebuggingUrl, string selector, string text)
    {
        ExecuteElementScript(
            remoteDebuggingUrl,
            selector,
            $"element.focus(); element.value = `${{element.value ?? ''}}{EscapeForJavaScriptTemplate(text)}`; element.dispatchEvent(new Event('input', {{ bubbles: true }})); element.dispatchEvent(new Event('change', {{ bubbles: true }})); return true;");
    }

    public void TypeProgressively(string remoteDebuggingUrl, string selector, string text, Action? afterCharacter = null)
    {
        Click(remoteDebuggingUrl, selector);

        foreach (var character in text)
        {
            Type(remoteDebuggingUrl, selector, character.ToString());
            afterCharacter?.Invoke();
            Thread.Sleep(80);
        }
    }

    public void Clear(string remoteDebuggingUrl, string selector)
    {
        ExecuteElementScript(
            remoteDebuggingUrl,
            selector,
            "element.focus(); element.value = ''; element.dispatchEvent(new Event('input', { bubbles: true })); element.dispatchEvent(new Event('change', { bubbles: true })); return true;");
    }

    public void Press(string remoteDebuggingUrl, string key)
    {
        Run(async () =>
        {
            await using var session = await OpenPrimaryPageSession(remoteDebuggingUrl);

            await session.SendCommand("Input.dispatchKeyEvent", writer =>
            {
                writer.WriteString("type", "keyDown");
                writer.WriteString("key", key);
            });

            await session.SendCommand("Input.dispatchKeyEvent", writer =>
            {
                writer.WriteString("type", "keyUp");
                writer.WriteString("key", key);
            });

            return true;
        });
    }

    public void Hover(string remoteDebuggingUrl, string selector)
    {
        ExecuteElementScript(
            remoteDebuggingUrl,
            selector,
            "const rect = element.getBoundingClientRect(); const options = { bubbles: true, clientX: rect.left + rect.width / 2, clientY: rect.top + rect.height / 2 }; element.dispatchEvent(new MouseEvent('mouseover', options)); element.dispatchEvent(new MouseEvent('mousemove', options)); return true;");
    }

    public void ScrollElementIntoView(string remoteDebuggingUrl, string selector)
    {
        ExecuteElementScript(
            remoteDebuggingUrl,
            selector,
            "element.scrollIntoView({ block: 'center', inline: 'center' }); return true;");
    }

    public void Select(string remoteDebuggingUrl, string selector, string value)
    {
        ExecuteElementScript(
            remoteDebuggingUrl,
            selector,
            $"element.value = {ToJsonStringLiteral(value)}; element.dispatchEvent(new Event('input', {{ bubbles: true }})); element.dispatchEvent(new Event('change', {{ bubbles: true }})); return true;");
    }

    public void ShowMessageBar(string remoteDebuggingUrl, string message)
    {
        Evaluate(
            remoteDebuggingUrl,
            $$"""
            (() => {
              const promote = element => {
                if (typeof element.showPopover === 'function') {
                  if (element.matches(':popover-open')) {
                    element.hidePopover();
                  }
                  element.showPopover();
                }
              };
              let bar = document.getElementById('__cmg_message_bar');
              if (!bar) {
                bar = document.createElement('div');
                bar.id = '__cmg_message_bar';
                bar.setAttribute('popover', 'manual');
                bar.setAttribute('role', 'status');
                bar.setAttribute('aria-live', 'polite');
                bar.style.all = 'initial';
                bar.style.position = 'fixed';
                bar.style.left = '50%';
                bar.style.top = '12px';
                bar.style.zIndex = '2147483646';
                bar.style.width = 'max-content';
                bar.style.maxWidth = 'min(760px, calc(100vw - 32px))';
                bar.style.minHeight = '42px';
                bar.style.margin = '0';
                bar.style.padding = '10px 16px';
                bar.style.border = '0';
                bar.style.background = '#111827';
                bar.style.color = '#ffffff';
                bar.style.font = '600 14px/1.45 system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif';
                bar.style.textAlign = 'center';
                bar.style.boxShadow = '0 10px 28px rgba(0,0,0,.24)';
                bar.style.boxSizing = 'border-box';
                bar.style.pointerEvents = 'none';
                bar.style.borderRadius = '8px';
                bar.style.transform = 'translateX(-50%)';
                bar.style.whiteSpace = 'pre-wrap';
                bar.style.overflowWrap = 'anywhere';

                const text = document.createElement('div');
                text.id = '__cmg_message_bar_text';
                bar.appendChild(text);

                document.documentElement.appendChild(bar);
              }

              const text = document.getElementById('__cmg_message_bar_text');
              text.textContent = {{ToJsonStringLiteral(message)}};
              promote(bar);
              return true;
            })()
            """);
    }

    public void PromoteMessageBar(string remoteDebuggingUrl)
    {
        Evaluate(
            remoteDebuggingUrl,
            """
            (() => {
              const bar = document.getElementById('__cmg_message_bar');
              if (!bar || typeof bar.showPopover !== 'function') return true;
              if (bar.matches(':popover-open')) {
                bar.hidePopover();
              }
              bar.showPopover();
              return true;
            })()
            """);
    }

    public string GetElementText(string remoteDebuggingUrl, string selector)
    {
        return EvaluateElementScript(remoteDebuggingUrl, selector, "return element.innerText ?? element.textContent ?? '';");
    }

    public string Evaluate(string remoteDebuggingUrl, string expression)
    {
        return Run(async () =>
        {
            await using var session = await OpenPrimaryPageSession(remoteDebuggingUrl);
            var response = await session.SendCommand("Runtime.evaluate", writer =>
            {
                writer.WriteString("expression", expression);
                writer.WriteBoolean("returnByValue", true);
            });

            if (!TryReadElement(response, ["result", "result"], out var result))
            {
                return string.Empty;
            }

            if (result.TryGetProperty("value", out var value))
            {
                return value.ValueKind is JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();
            }

            return result.TryGetProperty("description", out var description) ? description.GetString() ?? string.Empty : string.Empty;
        });
    }

    public void SetViewport(string remoteDebuggingUrl, int width, int height)
    {
        Run(async () =>
        {
            await using var session = await OpenPrimaryPageSession(remoteDebuggingUrl);
            await session.SendCommand("Emulation.setDeviceMetricsOverride", writer =>
            {
                writer.WriteNumber("width", width);
                writer.WriteNumber("height", height);
                writer.WriteNumber("deviceScaleFactor", 1);
                writer.WriteBoolean("mobile", false);
            });

            return true;
        });
    }

    public void DragAndDrop(string remoteDebuggingUrl, string sourceSelector, string targetSelector)
    {
        Run(async () =>
        {
            var pageTarget = await TryFindPageWithSelector(remoteDebuggingUrl, sourceSelector) ??
                throw new ElementNotFoundException(sourceSelector);

            await using var session = await DevToolsSession.Connect(pageTarget);
            var expression = BuildDragAndDropExpression(sourceSelector, targetSelector);
            var response = await session.SendCommand("Runtime.evaluate", writer =>
            {
                writer.WriteString("expression", expression);
                writer.WriteBoolean("returnByValue", true);
            });

            if (!TryReadBoolean(response, ["result", "result", "value"], out var success) || !success)
            {
                throw new ElementNotFoundException(targetSelector);
            }

            return true;
        });
    }

    public void MouseDragAndDrop(
        string remoteDebuggingUrl,
        string sourceSelector,
        string targetSelector,
        IReadOnlyList<ElementPoint> path,
        Action<ElementPoint>? afterMove = null)
    {
        Run(async () =>
        {
            var pageTarget = await TryFindPageWithSelector(remoteDebuggingUrl, sourceSelector) ??
                throw new ElementNotFoundException(sourceSelector);

            await using var session = await DevToolsSession.Connect(pageTarget);
            await session.ScrollElementIntoView(sourceSelector);
            await session.ScrollElementIntoView(targetSelector);

            var source = await GetElementClip(session, sourceSelector);
            _ = await GetElementClip(session, targetSelector);
            var start = new ElementPoint(source.CenterX, source.CenterY);
            var points = path.Count > 0 ? path : [start];

            await DispatchMouseMove(session, start, buttons: 0);
            await DispatchMousePressed(session, start);
            afterMove?.Invoke(start);

            foreach (var point in points)
            {
                await DispatchMouseMove(session, point, buttons: 1);
                afterMove?.Invoke(point);
            }

            await DispatchMouseReleased(session, points[^1]);

            return true;
        });
    }

    public void BeginPageDrag(string remoteDebuggingUrl, string sourceSelector, ElementPoint point)
    {
        Evaluate(
            remoteDebuggingUrl,
            $$"""
            (() => {
              const source = document.querySelector({{ToJsonStringLiteral(sourceSelector)}});
              if (!source) return false;
              const dataTransfer = new DataTransfer();
              let customDragImageSet = false;
              const originalSetDragImage = dataTransfer.setDragImage?.bind(dataTransfer);
              if (originalSetDragImage) {
                try {
                  Object.defineProperty(dataTransfer, 'setDragImage', {
                    configurable: true,
                    value: (...args) => {
                      customDragImageSet = true;
                      return originalSetDragImage(...args);
                    }
                  });
                } catch {
                }
              }
              window.__cmgRecordingDrag = { source, dataTransfer, defaultGhost: null };
              const eventOptions = {
                bubbles: true,
                cancelable: true,
                clientX: {{point.X.ToString(System.Globalization.CultureInfo.InvariantCulture)}},
                clientY: {{point.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}},
                dataTransfer
              };
              source.dispatchEvent(new DragEvent('dragstart', eventOptions));
              if (!customDragImageSet) {
                const existing = document.getElementById('__cmg_default_drag_ghost');
                existing?.remove();
                const sourceRect = source.getBoundingClientRect();
                const ghost = document.createElement('div');
                ghost.id = '__cmg_default_drag_ghost';
                ghost.setAttribute('popover', 'manual');
                ghost.style.all = 'initial';
                ghost.style.position = 'fixed';
                ghost.style.left = `${eventOptions.clientX}px`;
                ghost.style.top = `${eventOptions.clientY}px`;
                ghost.style.zIndex = '2147483645';
                ghost.style.width = `${Math.max(1, sourceRect.width)}px`;
                ghost.style.height = `${Math.max(1, sourceRect.height)}px`;
                ghost.style.margin = '0';
                ghost.style.padding = '0';
                ghost.style.border = '0';
                ghost.style.background = 'transparent';
                ghost.style.overflow = 'visible';
                ghost.style.pointerEvents = 'none';
                ghost.style.opacity = '.72';
                ghost.style.transform = 'translate(-50%, -50%) rotate(-1deg)';
                ghost.style.filter = 'drop-shadow(0 12px 18px rgba(0,0,0,.24))';
                const clone = source.cloneNode(true);
                clone.removeAttribute?.('id');
                clone.removeAttribute?.('popover');
                clone.style.pointerEvents = 'none';
                clone.style.boxSizing = 'border-box';
                clone.style.width = `${Math.max(1, sourceRect.width)}px`;
                clone.style.height = `${Math.max(1, sourceRect.height)}px`;
                clone.style.maxWidth = 'none';
                clone.style.maxHeight = 'none';
                clone.style.margin = '0';
                ghost.appendChild(clone);
                document.documentElement.appendChild(ghost);
                if (typeof ghost.showPopover === 'function') {
                  ghost.showPopover();
                }
                window.__cmgRecordingDrag.defaultGhost = ghost;
              }
              source.dispatchEvent(new DragEvent('drag', eventOptions));
              return true;
            })()
            """);
    }

    public void MovePageDrag(string remoteDebuggingUrl, ElementPoint point)
    {
        Evaluate(
            remoteDebuggingUrl,
            $$"""
            (() => {
              const state = window.__cmgRecordingDrag;
              if (!state?.source || !state?.dataTransfer) return false;
              const target = document.elementFromPoint(
                {{point.X.ToString(System.Globalization.CultureInfo.InvariantCulture)}},
                {{point.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}}) || document.body;
              const eventOptions = {
                bubbles: true,
                cancelable: true,
                clientX: {{point.X.ToString(System.Globalization.CultureInfo.InvariantCulture)}},
                clientY: {{point.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}},
                dataTransfer: state.dataTransfer
              };
              if (state.defaultGhost?.isConnected) {
                state.defaultGhost.style.left = `${eventOptions.clientX}px`;
                state.defaultGhost.style.top = `${eventOptions.clientY}px`;
                if (typeof state.defaultGhost.showPopover === 'function') {
                  if (state.defaultGhost.matches(':popover-open')) {
                    state.defaultGhost.hidePopover();
                  }
                  state.defaultGhost.showPopover();
                }
              }
              state.source.dispatchEvent(new DragEvent('drag', eventOptions));
              target.dispatchEvent(new DragEvent('dragenter', eventOptions));
              target.dispatchEvent(new DragEvent('dragover', eventOptions));
              return true;
            })()
            """);
    }

    public void EndPageDrag(string remoteDebuggingUrl, ElementPoint point)
    {
        Evaluate(
            remoteDebuggingUrl,
            $$"""
            (() => {
              const state = window.__cmgRecordingDrag;
              if (!state?.source || !state?.dataTransfer) return false;
              state.source.dispatchEvent(new DragEvent('dragend', {
                bubbles: true,
                cancelable: true,
                clientX: {{point.X.ToString(System.Globalization.CultureInfo.InvariantCulture)}},
                clientY: {{point.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}},
                dataTransfer: state.dataTransfer
              }));
              if (state.defaultGhost?.matches?.(':popover-open')) {
                state.defaultGhost.hidePopover();
              }
              state.defaultGhost?.remove();
              delete window.__cmgRecordingDrag;
              return true;
            })()
            """);
    }

    public void RemoveDefaultDragGhost(string remoteDebuggingUrl)
    {
        Evaluate(
            remoteDebuggingUrl,
            "(() => { const ghost = document.getElementById('__cmg_default_drag_ghost'); if (ghost?.matches?.(':popover-open')) ghost.hidePopover(); ghost?.remove(); return true; })()");
    }

    public byte[] GetPageScreenshot(string remoteDebuggingUrl, bool promoteMessageBar = true)
    {
        return Run(async () =>
        {
            await using var session = await OpenPrimaryPageSession(remoteDebuggingUrl);
            if (promoteMessageBar)
            {
                await PromoteMessageBar(session);
            }

            var screenshot = await session.SendCommand("Page.captureScreenshot", writer => writer.WriteString("format", "png"));

            if (!TryReadString(screenshot, "result", "data", out var data) || string.IsNullOrWhiteSpace(data))
            {
                throw new ChromeDevToolsException("Chrome did not return screenshot image data.");
            }

            return Convert.FromBase64String(data);
        });
    }

    public ElementPoint GetElementCenter(string remoteDebuggingUrl, string selector)
    {
        return Run(async () =>
        {
            var pageTarget = await TryFindPageWithSelector(remoteDebuggingUrl, selector) ??
                throw new ElementNotFoundException(selector);

            await using var session = await DevToolsSession.Connect(pageTarget);
            await session.ScrollElementIntoView(selector);
            var clip = await GetElementClip(session, selector);

            return new ElementPoint(clip.CenterX, clip.CenterY);
        });
    }

    public void MoveDomCursor(string remoteDebuggingUrl, ElementPoint point)
    {
        Evaluate(
            remoteDebuggingUrl,
            $$"""
            (() => {
              let cursor = document.getElementById('__cmg_virtual_cursor');
              if (!cursor) {
                cursor = document.createElement('div');
                cursor.id = '__cmg_virtual_cursor';
                cursor.setAttribute('popover', 'manual');
                cursor.style.all = 'initial';
                cursor.style.position = 'fixed';
                cursor.style.left = '0px';
                cursor.style.top = '0px';
                cursor.style.width = '26px';
                cursor.style.height = '34px';
                cursor.style.margin = '0';
                cursor.style.padding = '0';
                cursor.style.border = '0';
                cursor.style.background = 'transparent';
                cursor.style.overflow = 'visible';
                cursor.style.color = 'transparent';
                cursor.style.zIndex = '2147483647';
                cursor.style.pointerEvents = 'none';
                cursor.style.transform = 'translate(-3px, -3px)';
                cursor.style.filter = 'drop-shadow(0 2px 3px rgba(0,0,0,.4))';
                cursor.innerHTML = '<svg width="26" height="34" viewBox="0 0 26 34" xmlns="http://www.w3.org/2000/svg" aria-hidden="true"><path d="M3 2L22 19L13.2 20.1L9.2 31L3 2Z" fill="#fff" stroke="#111" stroke-width="2" stroke-linejoin="round"/><path d="M13.2 20.1L18.6 29.4" stroke="#111" stroke-width="2.5" stroke-linecap="round"/></svg>';
                document.documentElement.appendChild(cursor);
              }
              cursor.style.left = '{{point.X.ToString(System.Globalization.CultureInfo.InvariantCulture)}}px';
              cursor.style.top = '{{point.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}}px';
              if (typeof cursor.showPopover === 'function') {
                if (cursor.matches(':popover-open')) {
                  cursor.hidePopover();
                }
                cursor.showPopover();
              }
              return true;
            })()
            """);
    }

    public void RemoveDomCursor(string remoteDebuggingUrl)
    {
        Evaluate(
            remoteDebuggingUrl,
            "(() => { const cursor = document.getElementById('__cmg_virtual_cursor'); if (cursor?.matches?.(':popover-open')) cursor.hidePopover(); cursor?.remove(); return true; })()");
    }

    public IReadOnlyList<ChromePageTab> ListTabs(string remoteDebuggingUrl)
    {
        return Run(async () =>
        {
            var targets = await GetPageTargets(remoteDebuggingUrl);

            return targets
                .Select(target => new ChromePageTab(target.Id, target.Title, target.Url))
                .ToArray();
        });
    }

    public void ActivateTab(string remoteDebuggingUrl, int index)
    {
        Run(async () =>
        {
            var target = await GetPageTargetAt(remoteDebuggingUrl, index);
            using var httpClient = new HttpClient { Timeout = CommandTimeout };
            await httpClient.GetStringAsync($"{remoteDebuggingUrl.TrimEnd('/')}/json/activate/{target.Id}");

            return true;
        });
    }

    public void CloseTab(string remoteDebuggingUrl, int index)
    {
        Run(async () =>
        {
            var target = await GetPageTargetAt(remoteDebuggingUrl, index);
            using var httpClient = new HttpClient { Timeout = CommandTimeout };
            await httpClient.GetStringAsync($"{remoteDebuggingUrl.TrimEnd('/')}/json/close/{target.Id}");

            return true;
        });
    }

    private static T Run<T>(Func<Task<T>> action)
    {
        return action().GetAwaiter().GetResult();
    }

    private void ExecuteElementScript(string remoteDebuggingUrl, string selector, string body)
    {
        _ = EvaluateElementScript(remoteDebuggingUrl, selector, body);
    }

    private string EvaluateElementScript(string remoteDebuggingUrl, string selector, string body)
    {
        return Run(async () =>
        {
            var pageTarget = await TryFindPageWithSelector(remoteDebuggingUrl, selector) ??
                throw new ElementNotFoundException(selector);

            await using var session = await DevToolsSession.Connect(pageTarget);
            await session.ScrollElementIntoView(selector);

            var response = await session.SendCommand("Runtime.evaluate", writer =>
            {
                writer.WriteString("expression", BuildElementActionExpression(selector, body));
                writer.WriteBoolean("returnByValue", true);
            });

            if (!TryReadElement(response, ["result", "result"], out var result))
            {
                return string.Empty;
            }

            if (result.TryGetProperty("value", out var value))
            {
                return value.ValueKind is JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();
            }

            return result.TryGetProperty("description", out var description) ? description.GetString() ?? string.Empty : string.Empty;
        });
    }

    private static async Task<ElementClip> GetElementClip(DevToolsSession session, string selector)
    {
        var document = await session.SendCommand("DOM.getDocument");
        var rootNodeId = ReadInt32(document, "result", "root", "nodeId");

        var query = await session.SendCommand("DOM.querySelector", writer =>
        {
            writer.WriteNumber("nodeId", rootNodeId);
            writer.WriteString("selector", selector);
        });

        var nodeId = ReadInt32(query, "result", "nodeId");
        if (nodeId is 0)
        {
            throw new ElementNotFoundException(selector);
        }

        var boxModel = await session.SendCommand("DOM.getBoxModel", writer =>
        {
            writer.WriteNumber("nodeId", nodeId);
        });

        var clip = ElementClip.FromBoxModel(boxModel);
        if (clip.Width <= 0 || clip.Height <= 0)
        {
            throw new ChromeDevToolsException($"Element '{selector}' has no visible area.");
        }

        return clip;
    }

    private static async Task PromoteMessageBar(DevToolsSession session)
    {
        await session.SendCommand("Runtime.evaluate", writer =>
        {
            writer.WriteString(
                "expression",
                """
                (() => {
                  const bar = document.getElementById('__cmg_message_bar');
                  if (!bar || typeof bar.showPopover !== 'function') return true;
                  if (bar.matches(':popover-open')) {
                    bar.hidePopover();
                  }
                  bar.showPopover();
                  return true;
                })()
                """);
            writer.WriteBoolean("returnByValue", true);
        });
    }

    private static async Task ClickAt(DevToolsSession session, double x, double y)
    {
        await session.SendCommand("Input.dispatchMouseEvent", writer =>
        {
            writer.WriteString("type", "mousePressed");
            writer.WriteNumber("x", x);
            writer.WriteNumber("y", y);
            writer.WriteString("button", "left");
            writer.WriteNumber("clickCount", 1);
        });

        await session.SendCommand("Input.dispatchMouseEvent", writer =>
        {
            writer.WriteString("type", "mouseReleased");
            writer.WriteNumber("x", x);
            writer.WriteNumber("y", y);
            writer.WriteString("button", "left");
            writer.WriteNumber("clickCount", 1);
        });
    }

    private static Task DispatchMouseMove(DevToolsSession session, ElementPoint point, int buttons)
    {
        return session.SendCommand("Input.dispatchMouseEvent", writer =>
        {
            writer.WriteString("type", "mouseMoved");
            writer.WriteNumber("x", point.X);
            writer.WriteNumber("y", point.Y);
            writer.WriteNumber("buttons", buttons);
        });
    }

    private static Task DispatchMousePressed(DevToolsSession session, ElementPoint point)
    {
        return session.SendCommand("Input.dispatchMouseEvent", writer =>
        {
            writer.WriteString("type", "mousePressed");
            writer.WriteNumber("x", point.X);
            writer.WriteNumber("y", point.Y);
            writer.WriteString("button", "left");
            writer.WriteNumber("buttons", 1);
            writer.WriteNumber("clickCount", 1);
        });
    }

    private static Task DispatchMouseReleased(DevToolsSession session, ElementPoint point)
    {
        return session.SendCommand("Input.dispatchMouseEvent", writer =>
        {
            writer.WriteString("type", "mouseReleased");
            writer.WriteNumber("x", point.X);
            writer.WriteNumber("y", point.Y);
            writer.WriteString("button", "left");
            writer.WriteNumber("buttons", 0);
            writer.WriteNumber("clickCount", 1);
        });
    }

    private async Task<Uri?> TryFindPageWithSelector(string remoteDebuggingUrl, string selector)
    {
        var pageTargets = await GetPageWebSocketDebuggerUrls(remoteDebuggingUrl);

        foreach (var pageTarget in pageTargets)
        {
            await using var session = await DevToolsSession.Connect(pageTarget);
            var response = await session.SendCommand("Runtime.evaluate", writer =>
            {
                writer.WriteString("expression", $"Boolean(document.querySelector({ToJsonStringLiteral(selector)}))");
                writer.WriteBoolean("returnByValue", true);
            });

            if (TryReadBoolean(response, ["result", "result", "value"], out var exists) && exists)
            {
                return pageTarget;
            }
        }

        return null;
    }

    private static async Task<DevToolsSession> OpenPrimaryPageSession(string remoteDebuggingUrl)
    {
        var pageTargets = await GetPageWebSocketDebuggerUrls(remoteDebuggingUrl);

        return await DevToolsSession.Connect(pageTargets[0]);
    }

    private static async Task<IReadOnlyList<Uri>> GetPageWebSocketDebuggerUrls(string remoteDebuggingUrl)
    {
        return (await GetPageTargets(remoteDebuggingUrl))
            .Select(target => target.WebSocketDebuggerUrl)
            .ToArray();
    }

    private static async Task<IReadOnlyList<PageTarget>> GetPageTargets(string remoteDebuggingUrl)
    {
        using var httpClient = new HttpClient
        {
            Timeout = CommandTimeout
        };

        var targetsJson = await httpClient.GetStringAsync($"{remoteDebuggingUrl.TrimEnd('/')}/json");
        using var targets = JsonDocument.Parse(targetsJson);
        var pageTargets = new List<PageTarget>();

        foreach (var target in targets.RootElement.EnumerateArray())
        {
            if (!TryReadString(target, "type", out var type) ||
                !string.Equals(type, "page", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (TryReadString(target, "webSocketDebuggerUrl", out var webSocketDebuggerUrl) &&
                !string.IsNullOrWhiteSpace(webSocketDebuggerUrl) &&
                TryReadString(target, "id", out var id) &&
                !string.IsNullOrWhiteSpace(id))
            {
                _ = TryReadString(target, "title", out var title);
                _ = TryReadString(target, "url", out var url);
                pageTargets.Add(new PageTarget(id, title ?? string.Empty, url ?? string.Empty, new Uri(webSocketDebuggerUrl)));
            }
        }

        if (pageTargets.Count is 0)
        {
            throw new ChromeDevToolsException("No Chrome page target was available through remote debugging.");
        }

        return pageTargets;
    }

    private static async Task<PageTarget> GetPageTargetAt(string remoteDebuggingUrl, int index)
    {
        var targets = await GetPageTargets(remoteDebuggingUrl);
        if (index < 0 || index >= targets.Count)
        {
            throw new ChromeDevToolsException($"Tab index {index} does not exist. Available tab count: {targets.Count}.");
        }

        return targets[index];
    }

    private static string BuildOuterHtmlExpression(string selector)
    {
        return $"(() => {{ const element = document.querySelector({ToJsonStringLiteral(selector)}); return element ? element.outerHTML : null; }})()";
    }

    private static string BuildElementActionExpression(string selector, string body)
    {
        return $"(() => {{ const element = document.querySelector({ToJsonStringLiteral(selector)}); if (!element) return null; {body} }})()";
    }

    private static string BuildDragAndDropExpression(string sourceSelector, string targetSelector)
    {
        return $$"""
        (() => {
          const source = document.querySelector({{ToJsonStringLiteral(sourceSelector)}});
          const target = document.querySelector({{ToJsonStringLiteral(targetSelector)}});
          if (!source || !target) return false;
          const dataTransfer = new DataTransfer();
          source.dispatchEvent(new DragEvent('dragstart', { bubbles: true, cancelable: true, dataTransfer }));
          target.dispatchEvent(new DragEvent('dragenter', { bubbles: true, cancelable: true, dataTransfer }));
          target.dispatchEvent(new DragEvent('dragover', { bubbles: true, cancelable: true, dataTransfer }));
          target.dispatchEvent(new DragEvent('drop', { bubbles: true, cancelable: true, dataTransfer }));
          source.dispatchEvent(new DragEvent('dragend', { bubbles: true, cancelable: true, dataTransfer }));
          return true;
        })()
        """;
    }

    private static int ReadInt32(JsonElement root, params string[] path)
    {
        if (!TryReadElement(root, path, out var element) || !element.TryGetInt32(out var value))
        {
            throw new ChromeDevToolsException($"Chrome response did not contain expected field '{string.Join('.', path)}'.");
        }

        return value;
    }

    private static bool TryReadString(JsonElement root, string propertyName, out string? value)
    {
        value = null;

        if (!root.TryGetProperty(propertyName, out var element) || element.ValueKind is not JsonValueKind.String)
        {
            return false;
        }

        value = element.GetString();
        return true;
    }

    private static bool TryReadString(JsonElement root, IReadOnlyList<string> path, out string? value)
    {
        value = null;

        if (!TryReadElement(root, path, out var element) || element.ValueKind is JsonValueKind.Null)
        {
            return false;
        }

        if (element.ValueKind is not JsonValueKind.String)
        {
            return false;
        }

        value = element.GetString();
        return true;
    }

    private static bool TryReadBoolean(JsonElement root, IReadOnlyList<string> path, out bool value)
    {
        value = false;

        if (!TryReadElement(root, path, out var element) || element.ValueKind is not JsonValueKind.True and not JsonValueKind.False)
        {
            return false;
        }

        value = element.GetBoolean();
        return true;
    }

    private static bool TryReadDouble(JsonElement root, string propertyName, out double value)
    {
        value = 0;

        return root.TryGetProperty(propertyName, out var element) && element.TryGetDouble(out value);
    }

    private static string ToJsonStringLiteral(string value)
    {
        using var stream = new MemoryStream();

        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStringValue(value);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static string EscapeForJavaScriptTemplate(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("`", "\\`", StringComparison.Ordinal)
            .Replace("${", "\\${", StringComparison.Ordinal);
    }

    private static bool TryReadString(JsonElement root, string first, string second, out string? value)
    {
        value = null;

        if (!TryReadElement(root, [first, second], out var element) || element.ValueKind is not JsonValueKind.String)
        {
            return false;
        }

        value = element.GetString();
        return true;
    }

    private static bool TryReadElement(JsonElement root, IReadOnlyList<string> path, out JsonElement element)
    {
        element = root;

        foreach (var propertyName in path)
        {
            if (!element.TryGetProperty(propertyName, out element))
            {
                return false;
            }
        }

        return true;
    }

    private sealed class DevToolsSession : IAsyncDisposable
    {
        private readonly ClientWebSocket socket;
        private int commandId;

        private DevToolsSession(ClientWebSocket socket)
        {
            this.socket = socket;
        }

        public static async Task<DevToolsSession> Connect(Uri webSocketDebuggerUrl)
        {
            var socket = new ClientWebSocket();
            await socket.ConnectAsync(webSocketDebuggerUrl, CancellationToken.None);

            return new DevToolsSession(socket);
        }

        public async Task ScrollElementIntoView(string selector)
        {
            await SendCommand("Runtime.evaluate", writer =>
            {
                writer.WriteString("expression", BuildScrollIntoViewExpression(selector));
                writer.WriteBoolean("awaitPromise", true);
            });
        }

        public async Task<JsonElement> SendCommand(string method, Action<Utf8JsonWriter>? writeParams = null)
        {
            var id = Interlocked.Increment(ref commandId);
            using var commandStream = new MemoryStream();

            await using (var writer = new Utf8JsonWriter(commandStream))
            {
                writer.WriteStartObject();
                writer.WriteNumber("id", id);
                writer.WriteString("method", method);

                if (writeParams is not null)
                {
                    writer.WriteStartObject("params");
                    writeParams(writer);
                    writer.WriteEndObject();
                }

                writer.WriteEndObject();
            }

            await socket.SendAsync(commandStream.ToArray(), WebSocketMessageType.Text, true, CancellationToken.None);

            using var timeout = new CancellationTokenSource(CommandTimeout);

            while (true)
            {
                var response = await ReceiveMessage(timeout.Token);
                using var document = JsonDocument.Parse(response);

                if (!document.RootElement.TryGetProperty("id", out var responseId) ||
                    !responseId.TryGetInt32(out var responseCommandId) ||
                    responseCommandId != id)
                {
                    continue;
                }

                if (document.RootElement.TryGetProperty("error", out var error))
                {
                    throw new ChromeDevToolsException(ReadProtocolError(error));
                }

                return document.RootElement.Clone();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (socket.State is WebSocketState.Open)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "CMG command complete", CancellationToken.None);
            }

            socket.Dispose();
        }

        private async Task<string> ReceiveMessage(CancellationToken cancellationToken)
        {
            var buffer = new byte[16 * 1024];
            using var message = new MemoryStream();

            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(buffer, cancellationToken);
                message.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            return Encoding.UTF8.GetString(message.ToArray());
        }

        private static string BuildScrollIntoViewExpression(string selector)
        {
            return $"(() => {{ const element = document.querySelector({ToJsonStringLiteral(selector)}); if (!element) return false; element.scrollIntoView({{ block: 'center', inline: 'center' }}); return true; }})()";
        }

        private static string ReadProtocolError(JsonElement error)
        {
            if (error.TryGetProperty("message", out var message) &&
                message.ValueKind is JsonValueKind.String)
            {
                return message.GetString() ?? "Chrome DevTools Protocol error.";
            }

            return "Chrome DevTools Protocol error.";
        }
    }

    private readonly record struct ElementClip(double X, double Y, double Width, double Height)
    {
        public double CenterX => X + Width / 2;

        public double CenterY => Y + Height / 2;

        public static ElementClip FromBoxModel(JsonElement boxModel)
        {
            if (!TryReadElement(boxModel, ["result", "model", "content"], out var content) ||
                content.ValueKind is not JsonValueKind.Array)
            {
                throw new ChromeDevToolsException("Chrome did not return an element box model.");
            }

            var points = content.EnumerateArray().Select(point => point.GetDouble()).ToArray();
            if (points.Length < 8)
            {
                throw new ChromeDevToolsException("Chrome returned an invalid element box model.");
            }

            var xs = new[] { points[0], points[2], points[4], points[6] };
            var ys = new[] { points[1], points[3], points[5], points[7] };
            var x = xs.Min();
            var y = ys.Min();

            return new ElementClip(
                x,
                y,
                xs.Max() - x,
                ys.Max() - y);
        }
    }
}

public sealed class ChromeDevToolsException : Exception
{
    public ChromeDevToolsException(string message)
        : base(message)
    {
    }
}

public sealed class ElementNotFoundException : Exception
{
    public ElementNotFoundException(string selector)
        : base($"No element matched selector '{selector}'.")
    {
    }
}

public sealed record ChromePageTab(string Id, string Title, string Url);

public sealed record ElementPoint(double X, double Y);

internal sealed record PageTarget(string Id, string Title, string Url, Uri WebSocketDebuggerUrl);
