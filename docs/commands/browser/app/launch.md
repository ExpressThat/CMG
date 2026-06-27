# `browser app launch`

Launch an Electron or Windows WebView2 desktop app with remote debugging enabled, then make it the selected CMG browser target.

```powershell
cmg browser app launch <executable> [app-arguments...] [options]
cmg --edge browser app launch <executable> --kind webview2 --port 9333
```

## Arguments

- `<executable>`: Path to the compiled desktop app executable.
- `[app-arguments...]`: Additional arguments passed through to the app after CMG adds the debugging setup.

## Options

- `--kind <electron|webview2>`: App engine. Defaults to `electron`. `tauri` and `infiniframe` are accepted aliases for `webview2` because Windows builds use WebView2.
- `--port <port>`: Remote debugging port to expose. Defaults to `9222`.
- `--host <host>`: Remote debugging host CMG should store and probe. Defaults to `127.0.0.1`.
- `--connect-timeout <ms>`: Milliseconds to wait for `<host>:<port>/json` to expose a CDP page target. Defaults to `10000`. Use `0` to skip verification.

## Behavior

- `electron` adds `--remote-debugging-port=<port>` to the launched app.
- `webview2` sets `WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS` to include `--remote-debugging-port=<port>` before launching the app.
- `webview2` requires Windows. macOS WKWebView and Linux WebKitGTK do not expose Chromium CDP or WebDriver BiDi.
- CMG waits for the app's Chromium debugging endpoint before saving state. If the app starts but the endpoint is blocked, late, or not exposed, CMG exits with a reason instead of leaving stale browser state.
- On success, CMG stores the app's debugging URL in the selected Chrome or Edge state slot. Existing browser-control commands, scripts, `cmg run`, GIF recording, virtual pointer events, drag ghosts, captions, traces, and reports then target the app.
- On success, CMG arms page-side diagnostics capture for console messages and page errors. Captured diagnostics are stored in `window.__cmgConsole` and `window.__cmgPageErrors` and continue accumulating between CMG CLI invocations. Capture is forward-only; events before launch/diagnostics arming cannot be recovered.
- `--firefox` is rejected because this command needs a Chromium CDP endpoint.

## Stdout

On success:

```text
App launched for CMG. PID: <pid>.
Remote debugging: http://127.0.0.1:<port>
```

## Stderr

Validation errors are written to stderr, for example:

```text
App kind must be 'electron' or 'webview2'.
```

```text
App launched, but CMG could not connect to http://127.0.0.1:9222. Reason: <connection failure>
```

```text
WebView2 app control is only available on Windows. macOS WKWebView and Linux WebKitGTK do not expose CDP/BiDi.
```

## Exit Codes

- `0`: The app was launched and CMG state was updated.
- `1`: The app path, app kind, browser selector, port, platform, or process launch failed.

## Examples

```powershell
cmg browser app launch C:\Apps\ElectronDemo.exe --kind electron
cmg browser app launch C:\Apps\ElectronDemo.exe --kind electron -- --profile demo
cmg browser app launch C:\Apps\TauriDemo.exe --kind webview2 --port 9333
cmg --edge browser app launch C:\Apps\InfiniframeApp.exe --kind webview2 --port 9333
cmg browser app launch C:\Apps\ElectronDemo.exe --host localhost --connect-timeout 15000
```
