# `browser app`

Launch or attach CMG to a Chromium-based desktop app.

```powershell
cmg browser app [command] [options]
```

## Subcommands

- [`launch`](launch.md): Launch an Electron or Windows WebView2 app with remote debugging enabled.
- [`attach`](attach.md): Attach CMG to an already exposed Electron or WebView2 remote debugging port.

## Behavior

- Electron apps are supported on Windows, macOS, and Linux when the app accepts Chromium's `--remote-debugging-port=<port>` switch.
- Windows WebView2 apps, including Windows builds of Tauri and Infiniframe-style native webview apps, are supported with `WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS`.
- macOS WKWebView and Linux WebKitGTK apps do not expose CDP or WebDriver BiDi. CMG reports this instead of silently pretending those apps can use the Chromium path.
- Launch and attach verify the CDP endpoint by default, then write the selected Chrome or Edge state slot. Later `browser control`, `browser control script`, `cmg run`, GIF recording, traces, and reports use that app target.
- Use the top-level `--chrome` or `--edge` selector to choose which state slot is updated. The default is Chrome. `--firefox` is rejected because this app workflow uses Chromium CDP endpoints.

## Examples

```powershell
cmg browser app launch C:\Apps\ElectronDemo.exe --kind electron
cmg browser app launch C:\Apps\TauriDemo.exe --kind webview2 --port 9333
cmg browser app attach --port 9222
cmg browser app attach --host localhost --port 9222 --connect-timeout 15000
cmg --edge browser app attach --port 9333 --pid 1234
cmg browser control script --file demo.cmgscript --gif demo-output\app.gif
```
