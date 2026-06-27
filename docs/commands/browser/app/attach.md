# `browser app attach`

Attach CMG to an already-running Electron or Windows WebView2 app that exposes a Chromium remote debugging port.

```powershell
cmg browser app attach [options]
cmg --edge browser app attach --port 9333 --pid 1234
```

## Options

- `--port <port>`: Remote debugging port already exposed by the app. Defaults to `9222`.
- `--host <host>`: Remote debugging host. Defaults to `127.0.0.1`.
- `--connect-timeout <ms>`: Milliseconds to wait for `<host>:<port>/json` to expose a CDP page target. Defaults to `10000`. Use `0` to skip verification.
- `--pid <pid>`: Optional app process id for later close tracking. Defaults to `0`.

## Behavior

- Saves `http://<host>:<port>` into the selected Chrome or Edge state slot.
- Use this when an Electron app was started with `--remote-debugging-port=<port>` or a Windows WebView2 app was started with `WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS=--remote-debugging-port=<port>`.
- CMG verifies the remote debugging endpoint before saving state unless `--connect-timeout 0` is used.
- After attach, existing browser-control commands, scripts, `cmg run`, GIF recording, virtual pointer events, drag ghosts, captions, traces, and reports target the app.
- `--firefox` is rejected because this command needs a Chromium CDP endpoint.
- If the endpoint is not reachable, CMG reports the connection reason and leaves the previous browser state unchanged.

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

```text
Could not attach CMG to http://127.0.0.1:9222. Reason: <connection failure>
```

## Exit Codes

- `0`: CMG state was updated to point at the app endpoint.
- `1`: The browser selector, port, host, timeout, pid, or endpoint connection was invalid.

## Examples

```powershell
cmg browser app attach --port 9222
cmg browser app attach --host localhost --port 9222 --connect-timeout 15000
cmg --edge browser app attach --port 9333 --pid 1234
cmg browser control tabs list
cmg browser control script --file demo.cmgscript --gif demo-output\app.gif
```
