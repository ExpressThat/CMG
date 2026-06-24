# `browser launch`

Launches a CMG-controlled Chrome instance with remote debugging enabled.

```powershell
cmg browser launch [chrome-arguments...]
```

## Arguments

- `[chrome-arguments...]`: Additional arguments passed through to Chrome.

## Behavior

- Starts Chrome with `--remote-debugging-port=9222`.
- Uses a dedicated Chrome profile at `%LOCALAPPDATA%\CMG\chrome-profile`.
- Persists browser state at `%LOCALAPPDATA%\CMG\browser.state`.
- Only one CMG-controlled browser instance is launched. Calling this command again while the tracked process is running reports the existing process instead of opening another window.
- If no non-option argument is supplied, Chrome opens `about:blank`.

## Stdout

On first launch:

```text
Chrome launched for CMG. PID: <pid>.
Remote debugging: http://127.0.0.1:9222
```

When Chrome is already running:

```text
Chrome is already running for CMG. PID: <pid>.
Remote debugging: http://127.0.0.1:9222
```

## Exit Codes

- `0`: Chrome is running or was launched successfully.
- `1`: Chrome could not be found or launched.

## Examples

```powershell
cmg browser launch
cmg browser launch --window-size=1200,800
cmg browser launch https://example.com
```
