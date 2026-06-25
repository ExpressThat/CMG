# `browser launch`

Launches a CMG-controlled browser instance with remote debugging enabled. Chrome is the default. Use the optional top-level `--chrome` option to select Chrome explicitly, `--edge` to launch Microsoft Edge, or `--firefox` to launch Firefox.

```powershell
cmg browser launch [browser-arguments...] [options]
cmg --chrome browser launch [browser-arguments...]
cmg --edge browser launch [browser-arguments...]
cmg --firefox browser launch [browser-arguments...]
```

## Arguments

- `[browser-arguments...]`: Additional arguments passed through to the selected browser.

## Options

- `--headless`: Launch in headless mode. Chrome and Edge receive `--headless=new`; Firefox receives `--headless`.
- `--url <target>`: Initial URL or path to open. This is appended after raw browser arguments.

## Behavior

- Starts Chrome with `--remote-debugging-port=9222`, Edge with `--remote-debugging-port=9224`, or Firefox with `--remote-debugging-port 9223`.
- Uses a dedicated Chrome profile at `%LOCALAPPDATA%\CMG\chrome-profile`, Edge profile at `%LOCALAPPDATA%\CMG\edge-profile`, or Firefox profile at `%LOCALAPPDATA%\CMG\firefox-profile`.
- Persists Chrome state at `%LOCALAPPDATA%\CMG\browser.state`, Edge state at `%LOCALAPPDATA%\CMG\edge.browser.state`, or Firefox state at `%LOCALAPPDATA%\CMG\firefox.browser.state`.
- Only one CMG-controlled browser instance is launched. Calling this command again while the tracked process is running reports the existing process instead of opening another window.
- If no non-option argument is supplied, the browser opens `about:blank`.

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
- `1`: The selected browser could not be found or launched.

## Examples

```powershell
cmg browser launch
cmg --chrome browser launch
cmg browser launch --headless --url https://example.com
cmg browser launch --window-size=1200,800
cmg browser launch https://example.com
cmg --edge browser launch https://example.com
cmg --firefox browser launch https://example.com
```
