# `browser control assertions`

Page and element assertion command group.

```powershell
cmg browser control assertions [command] [options]
```

## Subcommands

- [`assertText`](assertText.md): Assert that an element contains text.
- [`expectText`](expectText.md): Assert that an element contains text.
- [`toHaveText`](toHaveText.md): Assert that an element contains text.
- [`toContainText`](toContainText.md): Assert that an element contains text.
- [`containsText`](containsText.md): Assert that an element contains text.
- [`waitForText`](waitForText.md): Wait until an element contains text.
- [`contains`](contains.md): Assert that the page body contains text.
- [`eval`](eval.md): Assert a JavaScript expression result.
- [`visible`](visible.md): Assert that an element is visible.
- [`hidden`](hidden.md): Assert that an element is hidden.
- [`enabled`](enabled.md): Assert that an element is enabled.
- [`disabled`](disabled.md): Assert that an element is disabled.
- [`value`](value.md): Assert that an element value contains text.
- [`attribute`](attribute.md): Assert that an element attribute contains text.
- [`checked`](checked.md): Assert that an element is checked or unchecked.
- [`count`](count.md): Assert the number of matching elements.

## Behavior

- Requires a browser started with [`browser launch`](../../launch.md).
- Runs the same underlying scripting actions as `browser control script`.
- Writes `PASS` output lines to stdout when assertions pass.
- Writes assertion, browser, selector, parse, or action errors to stderr.
- Exits `0` on success and `1` on failure.

## Examples

```powershell
cmg browser control assertions assertText "h1" "Ready"
cmg browser control assertions waitForText "#status" "Saved" --timeout 5000
cmg browser control assertions contains "Welcome"
cmg browser control assertions visible "#save" --timeout 5000
cmg browser control assertions eval "document.title" --equals "Checkout"
```
