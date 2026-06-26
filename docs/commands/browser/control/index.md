# `browser control`

Browser interaction and page control command group.

```powershell
cmg browser control [command] [options]
```

## Behavior

- Contains commands that interact with the active page in the selected browser. Chrome and Edge use Chrome DevTools Protocol; Firefox uses WebDriver BiDi.
- Run [`browser launch`](../launch.md) before using control commands. Include the same top-level browser option on launch and control commands when using `--edge` or `--firefox`.
- Running `browser control` without a subcommand exits with `1`.

## Subcommands

- [`script`](script.md): Run a `.cmgscript` browser automation script.
- [`validateScript`](validateScript.md): Validate a `.cmgscript` browser automation script without running it.
- [`navigation`](navigation/index.md): Navigation and page state commands.
- [`input`](input/index.md): Pointer, keyboard, and form input commands.
- [`tabs`](tabs/index.md): Tab and popup target commands.
- [`capture`](capture/index.md): Element and page capture commands.
- [`page`](page/index.md): Page evaluation, viewport, and utility commands.
- [`assertions`](assertions/index.md): Page and element assertion commands.
- [`storage`](storage/index.md): Storage and persisted browser state commands.
- [`network`](network/index.md): Network routing, waits, HAR, and environment commands.

## Examples

```powershell
cmg browser control --help
cmg --edge browser control --help
cmg --firefox browser control --help
cmg browser control capture getElement "h1" --html
cmg browser control validateScript --file flow.cmgscript
cmg browser control script --file flow.cmgscript
cmg browser control navigation title
cmg browser control navigation waitForLoadState complete
cmg browser control input click "#openProfileDialog"
cmg browser control tabs list
cmg browser control capture screenshot "#profileDialog" --output profile-dialog.png
cmg browser control page evaluate "document.title"
cmg browser control assertions assertText "h1" "Ready"
cmg browser control storage local set token abc
cmg browser control network waitForResponse "/api/profile" --status 200
```
