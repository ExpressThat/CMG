# `browser control`

Browser interaction and page control command group.

```powershell
cmg browser control [command] [options]
```

## Behavior

- Contains commands that interact with the active page through Chrome DevTools Protocol.
- Run [`browser launch`](../launch.md) before using control commands.
- Running `browser control` without a subcommand exits with `1`.

## Subcommands

- [`getElement`](getElement.md): Return HTML or a screenshot for a selected element.
- [`script`](script.md): Run a `.cmgscript` browser automation script.
- [`navigate`](navigate.md): Navigate the primary page target.
- [`waitForElement`](waitForElement.md): Wait until an element exists.
- [`click`](click.md): Click an element.
- [`type`](type.md): Type text into an element.
- [`clear`](clear.md): Clear an input-like element.
- [`press`](press.md): Press a keyboard key.
- [`hover`](hover.md): Hover an element.
- [`scrollIntoView`](scrollIntoView.md): Scroll an element into view.
- [`select`](select.md): Set a select-like element value.
- [`showMessageBar`](showMessageBar.md): Inject or update a fixed message bar at the top of the page.
- [`delay`](delay.md): Pause for a duration.
- [`html`](html.md): Print an element's outer HTML.
- [`screenshot`](screenshot.md): Capture an element screenshot.
- [`screenshotPage`](screenshotPage.md): Capture a full viewport screenshot.
- [`assertText`](assertText.md): Assert that an element contains text.
- [`evaluate`](evaluate.md): Evaluate JavaScript in the primary page.
- [`setViewport`](setViewport.md): Set viewport dimensions.
- [`dragAndDrop`](dragAndDrop.md): Drag one element onto another.
- [`listTabs`](listTabs.md): List available page targets.
- [`activateTab`](activateTab.md): Activate a tab by index.
- [`closeTab`](closeTab.md): Close a tab by index.
- [`set`](set.md): Run the script variable action once.

## Examples

```powershell
cmg browser control --help
cmg browser control getElement "h1" --html
cmg browser control script --file flow.cmgscript
cmg browser control click "#openProfileDialog"
cmg browser control screenshot "#profileDialog" --output profile-dialog.png
```
