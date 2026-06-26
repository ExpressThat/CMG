# `browser control input`

Pointer, keyboard, and form input command group.

```powershell
cmg browser control input [command] [options]
```

## Subcommands

- [`waitForElement`](waitForElement.md): Wait until an element exists.
- [`click`](click.md): Click an element.
- [`doubleClick`](doubleClick.md): Double-click an element.
- [`rightClick`](rightClick.md): Right-click an element.
- [`tap`](tap.md): Tap an element with touch-style events.
- [`type`](type.md): Type text into an element.
- [`fill`](fill.md): Replace an input-like element value.
- [`clear`](clear.md): Clear an input-like element.
- [`check`](check.md): Check a checkbox-like element.
- [`uncheck`](uncheck.md): Uncheck a checkbox-like element.
- [`focus`](focus.md): Focus an element.
- [`blur`](blur.md): Blur an element.
- [`selectText`](selectText.md): Select text inside an element.
- [`press`](press.md): Press a keyboard key.
- [`keyDown`](keyDown.md): Dispatch a keydown event.
- [`keyUp`](keyUp.md): Dispatch a keyup event.
- [`insertText`](insertText.md): Insert text at the active element.
- [`hover`](hover.md): Hover an element.
- [`scrollIntoView`](scrollIntoView.md): Scroll an element into view.
- [`select`](select.md): Set a select-like element value.
- [`dragAndDrop`](dragAndDrop.md): Drag one element onto another.

## Behavior

- Requires a browser started with [`browser launch`](../../launch.md).
- Runs the same underlying scripting actions as `browser control script`.
- Selector arguments accept CSS selectors and CMG rich locators.
- Writes `PASS` and action output lines to stdout.
- Writes browser, selector, parse, or action errors to stderr.
- Exits `0` on success and `1` on failure.

## Examples

```powershell
cmg browser control input click "#save"
cmg browser control input fill "#name" "CMG Test"
cmg browser control input keyDown Shift
cmg browser control input dragAndDrop ".card" "#dropZone"
```
