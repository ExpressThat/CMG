# `browser app attach`

Attach CMG to an already-running Electron or Windows WebView2 app that exposes a Chromium remote debugging port.

```powershell
cmg browser app attach [options]
cmg --edge browser app attach --port 9333 --pid 1234
```

## Options

- `--port <port>`: Remote debugging port already exposed by the app. Defaults to `9222`.
- `--pid <pid>`: Optional app process id for later close tracking. Defaults to `0`.

## Behavior

- Saves `http://127.0.0.1:<port>` into the selected Chrome or Edge state slot.
- Use this when an Electron app was started with `--remote-debugging-port=<port>` or a Windows WebView2 app was started with `WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS=--remote-debugging-port=<port>`.
- After attach, existing browser-control commands, scripts, `cmg run`, GIF recording, virtual pointer events, drag ghosts, captions, traces, and reports target the app.
- `--firefox` is rejected because this command needs a Chromium CDP endpoint.
- CMG does not verify every page target during attach; the first control command reports connection details if the app endpoint is not reachable.

## Stdout

On success:

```text
Attached CMG to app debugging endpoint on port <port>.
Remote debugging: http://127.0.0.1:<port>
```

## Stderr

Validation errors are written to stderr, for example:

```text
--port must be between 1 and 65535.
```

## Exit Codes

- `0`: CMG state was updated to point at the app endpoint.
- `1`: The browser selector, port, or pid value was invalid.

## Examples

```powershell
cmg browser app attach --port 9222
cmg --edge browser app attach --port 9333 --pid 1234
cmg browser control tabs list
cmg browser control script --file demo.cmgscript --gif demo-output\app.gif
```
