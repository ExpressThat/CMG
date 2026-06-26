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
- [`assertVisible`](assertVisible.md): Assert that an element exists and is visible.
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
- [`expectAttached`](expectAttached.md): Assert that an element is attached.
- [`toBeAttached`](toBeAttached.md): Playwright-style alias for `expectAttached`.
- [`expectDetached`](expectDetached.md): Assert that an element is detached.
- [`toBeDetached`](toBeDetached.md): Playwright-style alias for `expectDetached`.
- [`expectEditable`](expectEditable.md): Assert that an element is editable.
- [`toBeEditable`](toBeEditable.md): Playwright-style alias for `expectEditable`.
- [`expectEmpty`](expectEmpty.md): Assert that an element is empty.
- [`toBeEmpty`](toBeEmpty.md): Playwright-style alias for `expectEmpty`.
- [`expectFocused`](expectFocused.md): Assert that an element is focused.
- [`toBeFocused`](toBeFocused.md): Playwright-style alias for `expectFocused`.
- [`expectInViewport`](expectInViewport.md): Assert that an element intersects the viewport.
- [`toBeInViewport`](toBeInViewport.md): Playwright-style alias for `expectInViewport`.
- [`value`](value.md): Assert that an element value contains text.
- [`expectValue`](expectValue.md): Exact scripting alias for `value`.
- [`toHaveValue`](toHaveValue.md): Exact scripting alias for `value`.
- [`expectValues`](expectValues.md): Assert that selected values match in order.
- [`toHaveValues`](toHaveValues.md): Playwright-style alias for `expectValues`.
- [`attribute`](attribute.md): Assert that an element attribute contains text.
- [`expectAttribute`](expectAttribute.md): Exact scripting alias for `attribute`.
- [`toHaveAttribute`](toHaveAttribute.md): Exact scripting alias for `attribute`.
- [`expectClass`](expectClass.md): Assert that an element class contains a value.
- [`toHaveClass`](toHaveClass.md): Playwright-style alias for `expectClass`.
- [`expectId`](expectId.md): Assert that an element id equals a value.
- [`toHaveId`](toHaveId.md): Playwright-style alias for `expectId`.
- [`expectCSS`](expectCSS.md): Assert that a computed CSS property contains a value.
- [`toHaveCSS`](toHaveCSS.md): Playwright-style alias for `expectCSS`.
- [`expectProperty`](expectProperty.md): Assert that a DOM property contains a value.
- [`toHaveJSProperty`](toHaveJSProperty.md): Playwright-style alias for `expectProperty`.
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
cmg browser control assertions assertVisible "#save" --timeout 5000
cmg browser control assertions visible "#save" --timeout 5000
cmg browser control assertions toBeVisible "#save"
cmg browser control assertions toBeEditable "#name"
cmg browser control assertions toBeInViewport "#save"
cmg browser control assertions toHaveValue "#name" "Ada"
cmg browser control assertions toHaveValues "#plans" "basic" "pro"
cmg browser control assertions toHaveClass "#save" "ready"
cmg browser control assertions toHaveCSS "#save" "display" "block"
cmg browser control assertions eval "document.title" --equals "Checkout"
cmg browser control assertions expectExpression "window.appReady" --timeout 5000
```
