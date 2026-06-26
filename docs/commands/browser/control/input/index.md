# `browser control input`

Pointer, keyboard, and form input command group.

```powershell
cmg browser control input [command] [options]
```

## Subcommands

- [`waitForElement`](waitForElement.md): Wait until an element exists.
- [`click`](click.md): Click an element, with optional button, click count, modifier, delay, and element-offset controls.
- [`dblclick`](dblclick.md): Double-click an element, with optional modifier and element-offset controls.
- [`doubleClick`](doubleClick.md): Double-click an element, with optional modifier and element-offset controls.
- [`rightClick`](rightClick.md): Right-click an element, with optional modifier and element-offset controls.
- [`contextClick`](contextClick.md): Right-click an element, with optional modifier and element-offset controls.
- [`tap`](tap.md): Tap an element with touch-style events.
- [`touchTap`](touchTap.md): Tap an element with touch-style events.
- [`type`](type.md): Type text into an element, optionally with per-character delay.
- [`pressSequentially`](pressSequentially.md): Type text into an element using sequential key presses, optionally with per-character delay.
- [`fill`](fill.md): Replace an input-like element value.
- [`clear`](clear.md): Clear an input-like element.
- [`check`](check.md): Check a checkbox-like element.
- [`uncheck`](uncheck.md): Uncheck a checkbox-like element.
- [`focus`](focus.md): Focus an element.
- [`blur`](blur.md): Blur an element.
- [`selectText`](selectText.md): Select text inside an element.
- [`press`](press.md): Press a keyboard key or shortcut chord, optionally with key hold delay.
- [`shortcut`](shortcut.md): Press a keyboard shortcut chord.
- [`hotkey`](hotkey.md): Press a keyboard shortcut chord.
- [`keyboardShortcut`](keyboardShortcut.md): Press a keyboard shortcut chord.
- [`keyDown`](keyDown.md): Dispatch a keydown event.
- [`keyUp`](keyUp.md): Dispatch a keyup event.
- [`insertText`](insertText.md): Insert text at the active element.
- [`hover`](hover.md): Hover an element, with optional modifier and element-offset controls.
- [`scrollIntoView`](scrollIntoView.md): Scroll an element into view.
- [`select`](select.md): Set a select-like element value by value, label, or index.
- [`selectOption`](selectOption.md): Set a select-like element value by value, label, or index.
- [`dragAndDrop`](dragAndDrop.md): Drag one element onto another, optionally using source and target offsets.
- [`dragTo`](dragTo.md): Drag one element onto another, optionally using source and target offsets.
- [`mouse`](mouse/index.md): Low-level mouse movement and button commands.
- [`scroll`](scroll/index.md): Window, element, and wheel scrolling commands.
- [`clipboard`](clipboard/index.md): Page-side clipboard shim commands.
- [`dispatchEvent`](dispatchEvent.md): Dispatch an Event or CustomEvent on an element.
- [`uploadFiles`](uploadFiles.md): Assign files to an input[type=file] element.
- [`setInputFiles`](setInputFiles.md): Assign files to an input[type=file] element.
- [`selectFile`](selectFile.md): Assign files to an input[type=file] element.

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
cmg browser control input dblclick "#canvas" --x 12 --y 8
cmg browser control input fill "#name" "CMG Test"
cmg browser control input pressSequentially "#name" "CMG"
cmg browser control input shortcut Control+S
cmg browser control input keyDown Shift
cmg browser control input dragAndDrop ".card" "#dropZone"
cmg browser control input dragTo ".card" "#dropZone"
cmg browser control input mouse move center
cmg browser control input scroll wheel "#pane" --delta-y 120
cmg browser control input uploadFiles "#avatar" fixtures/avatar.png
```
