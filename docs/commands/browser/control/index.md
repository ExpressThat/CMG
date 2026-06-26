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

- [`getElement`](getElement.md): Return HTML or a screenshot for a selected element.
- [`script`](script.md): Run a `.cmgscript` browser automation script.
- [`validateScript`](validateScript.md): Validate a `.cmgscript` browser automation script without running it.
- [`navigation`](navigation/index.md): Navigation and page state commands.
- [`input`](input/index.md): Pointer, keyboard, and form input commands.
- [`tabs`](tabs/index.md): Tab and popup target commands.
- [`showMessageBar`](showMessageBar.md): Inject or update a fixed message bar at the top of the page.
- [`delay`](delay.md): Pause for a duration.
- [`html`](html.md): Print an element's outer HTML.
- [`screenshot`](screenshot.md): Capture an element screenshot.
- [`screenshotPage`](screenshotPage.md): Capture a viewport or full-page screenshot.
- [`assertText`](assertText.md): Assert that an element contains text.
- [`evaluate`](evaluate.md): Evaluate JavaScript in the primary page.
- [`setViewport`](setViewport.md): Set viewport dimensions.
- [`set`](set.md): Run the script variable action once.

## Examples

```powershell
cmg browser control --help
cmg --edge browser control --help
cmg --firefox browser control --help
cmg browser control getElement "h1" --html
cmg browser control validateScript --file flow.cmgscript
cmg browser control script --file flow.cmgscript
cmg browser control navigation title
cmg browser control navigation waitForLoadState complete
cmg browser control input click "#openProfileDialog"
cmg browser control tabs list
cmg browser control screenshot "#profileDialog" --output profile-dialog.png
```
