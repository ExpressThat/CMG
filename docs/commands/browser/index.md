# `browser`

Browser lifecycle and capture command group.

```powershell
cmg browser [command] [options]
```

## Options

- `--port <port>`: Select the remote debugging port for `launch`, `close`, and all `control` commands. Defaults to Chrome `9222`, Edge `9224`, and Firefox `9223`. Use this to run or control multiple same-browser CMG instances at once. App commands use their own endpoint `--port` option.

## Behavior

- Browser state is keyed by selected browser and port. `cmg browser --port 9333 launch` creates a separate Chrome profile and state slot from the default `cmg browser launch`.
- Use the same browser selector and port on later commands, for example `cmg browser --port 9333 control page evaluate "document.title"`.
- The default port keeps the default state paths for normal single-browser use.

## Subcommands

- [`launch`](launch.md): Launch a CMG-controlled browser instance with remote debugging enabled.
- [`app`](app/index.md): Launch or attach to Chromium-based desktop apps such as Electron and Windows WebView2 apps.
- [`close`](close.md): Close the CMG-controlled browser instance.
- [`lease`](lease/index.md): Inspect, renew, or disable opt-in idle cleanup for a CMG-owned headless browser.
- [`control`](control/index.md): Browser interaction and page control commands.

## Examples

```powershell
cmg browser launch
cmg --chrome browser launch
cmg --edge browser launch
cmg --firefox browser launch
cmg browser --port 9333 launch --headless
cmg browser --port 9333 lease status
cmg browser --port 9333 lease keepAlive
cmg browser --port 9333 control page evaluate "document.title"
cmg browser --port 9333 close
cmg browser app launch C:\Apps\DesktopApp.exe --kind electron
cmg browser app attach --port 9222
cmg browser close
cmg browser control --help
```
