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
- [`expectEval`](expectEval.md): Assert a JavaScript expression result.
- [`assertEval`](assertEval.md): Assert a JavaScript expression result.
- [`expectExpression`](expectExpression.md): Assert a JavaScript expression result.
- [`assertExpression`](assertExpression.md): Assert a JavaScript expression result.
- [`visible`](visible.md): Assert that an element is visible.
- [`hidden`](hidden.md): Assert that an element is hidden.
- [`enabled`](enabled.md): Assert that an element is enabled.
- [`disabled`](disabled.md): Assert that an element is disabled.
- [`expectVisible`](expectVisible.md): Exact scripting alias for `visible`.
- [`toBeVisible`](toBeVisible.md): Exact scripting alias for `visible`.
- [`waitForVisible`](waitForVisible.md): Wait until an element is visible.
- [`expectHidden`](expectHidden.md): Exact scripting alias for `hidden`.
- [`toBeHidden`](toBeHidden.md): Exact scripting alias for `hidden`.
- [`waitForHidden`](waitForHidden.md): Wait until an element is hidden.
- [`expectEnabled`](expectEnabled.md): Exact scripting alias for `enabled`.
- [`toBeEnabled`](toBeEnabled.md): Exact scripting alias for `enabled`.
- [`expectDisabled`](expectDisabled.md): Exact scripting alias for `disabled`.
- [`toBeDisabled`](toBeDisabled.md): Exact scripting alias for `disabled`.
- [`value`](value.md): Assert that an element value contains text.
- [`expectValue`](expectValue.md): Exact scripting alias for `value`.
- [`toHaveValue`](toHaveValue.md): Exact scripting alias for `value`.
- [`attribute`](attribute.md): Assert that an element attribute contains text.
- [`expectAttribute`](expectAttribute.md): Exact scripting alias for `attribute`.
- [`toHaveAttribute`](toHaveAttribute.md): Exact scripting alias for `attribute`.
- [`checked`](checked.md): Assert that an element is checked or unchecked.
- [`expectChecked`](expectChecked.md): Exact scripting alias for `checked`.
- [`toBeChecked`](toBeChecked.md): Exact scripting alias for `checked`.
- [`count`](count.md): Assert the number of matching elements.
- [`expectCount`](expectCount.md): Exact scripting alias for `count`.
- [`toHaveCount`](toHaveCount.md): Exact scripting alias for `count`.

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
cmg browser control assertions toBeVisible "#save"
cmg browser control assertions toHaveValue "#name" "Ada"
cmg browser control assertions eval "document.title" --equals "Checkout"
cmg browser control assertions expectExpression "window.appReady" --timeout 5000
```
