# `browser control frames`

Same-origin iframe interaction commands.

```powershell
cmg browser control frames [command] [options]
```

## Subcommands

- [`click`](click.md): Click an element inside an iframe.
- [`frameClick`](frameClick.md): Click an element inside an iframe.
- [`hover`](hover.md): Hover an element inside an iframe.
- [`frameHover`](frameHover.md): Hover an element inside an iframe.
- [`type`](type.md): Type text into an element inside an iframe.
- [`frameType`](frameType.md): Type text into an element inside an iframe.
- [`fill`](fill.md): Fill an element inside an iframe.
- [`frameFill`](frameFill.md): Fill an element inside an iframe.
- [`assertText`](assertText.md): Assert text inside an iframe element.
- [`frameAssertText`](frameAssertText.md): Assert text inside an iframe element.
- [`expectText`](expectText.md): Provider-style frame text assertion alias.
- [`frameExpectText`](frameExpectText.md): Explicit provider-style frame text assertion alias.
- [`toHaveText`](toHaveText.md): Provider-style frame text assertion alias.
- [`frameToHaveText`](frameToHaveText.md): Explicit provider-style frame text assertion alias.
- [`toContainText`](toContainText.md): Provider-style frame text containment alias.
- [`frameToContainText`](frameToContainText.md): Explicit provider-style frame text containment alias.
- [`contains`](contains.md): Cypress-style frame text containment alias.
- [`frameContains`](frameContains.md): Explicit Cypress-style frame text containment alias.
- [`waitForElement`](waitForElement.md): Wait for an element inside an iframe.
- [`frameWaitForElement`](frameWaitForElement.md): Wait for an element inside an iframe.
- [`waitForSelector`](waitForSelector.md): Provider-style frame selector wait alias.
- [`frameWaitForSelector`](frameWaitForSelector.md): Explicit provider-style frame selector wait alias.
- [`evaluate`](evaluate.md): Evaluate JavaScript inside an iframe.
- [`frameEvaluate`](frameEvaluate.md): Evaluate JavaScript inside an iframe.

## Behavior

- Requires a browser started with [`browser launch`](../../launch.md).
- The iframe must be same-origin and ready.
- Runs the same underlying scripting actions as `browser control script`.
- Script GIF recordings move the virtual pointer to the top-page coordinate inside the iframe for frame click, hover, type, and fill actions.
- Writes `PASS` and `FRAME` or `FRAME_EVALUATE` output lines to stdout.
- Writes browser, frame, selector, parse, or action errors to stderr.
- Exits `0` on success and `1` on failure.

## Examples

```powershell
cmg browser control frames waitForElement "#checkoutFrame" "#email" --timeout 5000
cmg browser control frames waitForSelector "#checkoutFrame" "#status" --timeout 5000
cmg browser control frames fill "#checkoutFrame" "#email" "agent@example.com"
cmg browser control frames click "#checkoutFrame" "#save"
cmg browser control frames assertText "#checkoutFrame" "#status" "^Saved$" --match regex --ignore-case
cmg browser control frames toContainText "#checkoutFrame" "#status" "Saved"
```
