# `browser`

Browser lifecycle and capture command group.

```powershell
cmg browser [command] [options]
```

## Subcommands

- [`launch`](launch.md): Launch a CMG-controlled browser instance with remote debugging enabled.
- [`app`](app/index.md): Launch or attach to Chromium-based desktop apps such as Electron and Windows WebView2 apps.
- [`close`](close.md): Close the CMG-controlled browser instance.
- [`control`](control/index.md): Browser interaction and page control commands.

## Examples

```powershell
cmg browser launch
cmg --chrome browser launch
cmg --edge browser launch
cmg --firefox browser launch
cmg browser app launch C:\Apps\DesktopApp.exe --kind electron
cmg browser app attach --port 9222
cmg browser close
cmg browser control --help
```
