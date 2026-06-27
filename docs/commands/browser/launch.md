# `browser launch`

Launches a CMG-controlled browser instance with remote debugging enabled. Chrome is the default. Use the optional top-level `--chrome` option to select Chrome explicitly, `--edge` to launch Microsoft Edge, or `--firefox` to launch Firefox.

```powershell
cmg browser launch [browser-arguments...] [options]
cmg browser --port <port> launch [browser-arguments...] [options]
cmg --chrome browser launch [browser-arguments...]
cmg --edge browser launch [browser-arguments...]
cmg --firefox browser launch [browser-arguments...]
```

## Arguments

- `[browser-arguments...]`: Additional arguments passed through to the selected browser.

## Options

- `--headless`: Launch in headless mode. Chrome and Edge receive `--headless=new`; Firefox receives `--headless`.
- `--url <target>`: Initial URL or path to open. This is appended after raw browser arguments.

## Browser Group Options

- `browser --port <port>`: Remote debugging port for this browser instance. Defaults to Chrome `9222`, Edge `9224`, and Firefox `9223`. Put this after `browser` and before `launch`.

## Behavior

- Starts Chrome with `--remote-debugging-port=9222`, Edge with `--remote-debugging-port=9224`, or Firefox with `--remote-debugging-port 9223` unless `browser --port <port>` is provided.
- Uses a dedicated profile per browser and port. Default ports keep `%LOCALAPPDATA%\CMG\chrome-profile`, `%LOCALAPPDATA%\CMG\edge-profile`, or `%LOCALAPPDATA%\CMG\firefox-profile`; custom ports use a port-suffixed profile directory.
- Persists browser state per browser and port. Default ports keep the standard state files; custom ports use port-suffixed state files.
- Only one CMG-controlled browser instance is launched per browser and port. Calling this command again for the same browser and port reports the existing process. Calling it with a different port can launch another same-browser instance.
- If no non-option argument is supplied, the browser opens `about:blank`.
- On successful launch, CMG arms page-side diagnostics capture for console messages and page errors. Captured diagnostics are stored in `window.__cmgConsole` and `window.__cmgPageErrors` and continue accumulating between CMG CLI invocations. Capture is forward-only; events before launch/diagnostics arming cannot be recovered.

## Stdout

On first launch:

```text
Chrome launched for CMG. PID: <pid>.
Remote debugging: http://127.0.0.1:9222
```

Firefox launch writes:

```text
Firefox launched for CMG. PID: <pid>.
Remote debugging: ws://127.0.0.1:9223/session
```

Edge launch writes:

```text
Edge launched for CMG. PID: <pid>.
Remote debugging: http://127.0.0.1:9224
```

When Chrome is already running:

```text
Chrome is already running for CMG. PID: <pid>.
Remote debugging: http://127.0.0.1:9222
```

## Exit Codes

- `0`: The selected browser is running or was launched successfully.
- `1`: The selected browser could not be found or launched, or `browser --port` is outside `1..65535`.

## Examples

```powershell
cmg browser launch
cmg --chrome browser launch
cmg browser --port 9333 launch --headless
cmg browser --port 9333 launch --url https://example.com
cmg browser launch --headless --url https://example.com
cmg browser launch --window-size=1200,800
cmg browser launch https://example.com
cmg --edge browser launch https://example.com
cmg --firefox browser launch https://example.com
```
