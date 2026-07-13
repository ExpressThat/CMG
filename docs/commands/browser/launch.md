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
- `--idle-timeout <duration>`: Opt into conservative cleanup for this CMG-launched headless browser. Accepts positive `ms`, `s`, `m`, or `h` durations such as `30m` or `2h`, and requires `--headless`. Cleanup is disabled when this option and `CMG_BROWSER_IDLE_TIMEOUT` are both absent.
- `--no-idle-cleanup`: Explicitly disable an existing lease for the selected browser and port. This cannot be combined with `--idle-timeout`.

## Browser Group Options

- `browser --port <port>`: Remote debugging port for this browser instance. Defaults to Chrome `9222`, Edge `9224`, and Firefox `9223`. Put this after `browser` and before `launch`.

## Behavior

- Starts Chrome with `--remote-debugging-port=9222`, Edge with `--remote-debugging-port=9224`, or Firefox with `--remote-debugging-port 9223` unless `browser --port <port>` is provided.
- Uses a dedicated profile per browser and port. Default ports keep `%LOCALAPPDATA%\CMG\chrome-profile`, `%LOCALAPPDATA%\CMG\edge-profile`, or `%LOCALAPPDATA%\CMG\firefox-profile`; custom ports use a port-suffixed profile directory.
- Persists browser state per browser and port. Default ports keep the standard state files; custom ports use port-suffixed state files.
- Only one CMG-controlled browser instance is launched per browser and port. Calling this command again for the same browser and port reports the existing process. Calling it with a different port can launch another same-browser instance.
- If no non-option argument is supplied, the browser opens `about:blank`.
- On successful launch, CMG arms page-side diagnostics capture for console messages and page errors. Captured diagnostics are stored in `window.__cmgConsole` and `window.__cmgPageErrors` and continue accumulating between CMG CLI invocations. Capture is forward-only; events before launch/diagnostics arming cannot be recovered.
- Idle cleanup is opt-in and only available for a CMG-launched headless browser. CMG records an ownership token and process start time, starts a lightweight monitor, and renews the lease during browser-control scripts, one-shot controls, and test runs. It never applies this policy to visible, attached, or user-launched browsers.
- The environment variable `CMG_BROWSER_IDLE_TIMEOUT` provides the same launch default as `--idle-timeout`. An explicit CLI timeout wins. Use a generous duration that allows the agent to reason, edit files, and inspect reports before returning.

## Stdout

On first launch:

```text
Chrome launched for CMG. PID: <pid>.
Remote debugging: http://127.0.0.1:9222
```

An opted-in idle lease adds:

```text
BROWSER_IDLE_LEASE status=scheduled browser=chrome port=9222 pid=<pid> ownership=cmg idleTimeoutMs=<milliseconds> deadline=<ISO-8601> reason=enabled
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
- `1`: The selected browser could not be found or launched, `browser --port` is outside `1..65535`, the duration is invalid, cleanup was requested for a visible/unowned browser, or the monitor could not start.

## Examples

```powershell
cmg browser launch
cmg --chrome browser launch
cmg browser --port 9333 launch --headless
cmg browser --port 9333 launch --url https://example.com
cmg browser launch --headless --url https://example.com
cmg browser --port 9333 launch --headless --idle-timeout 45m
cmg browser --port 9333 launch --headless --no-idle-cleanup
cmg browser launch --window-size=1200,800
cmg browser launch https://example.com
cmg --edge browser launch https://example.com
cmg --firefox browser launch https://example.com
```
