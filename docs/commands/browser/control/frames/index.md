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
- [`textContent`](textContent.md): Read iframe element `textContent`.
- [`frameTextContent`](frameTextContent.md): Read iframe element `textContent`.
- [`innerText`](innerText.md): Read iframe element `innerText`.
- [`frameInnerText`](frameInnerText.md): Read iframe element `innerText`.
- [`inputValue`](inputValue.md): Read iframe input-like element value.
- [`frameInputValue`](frameInputValue.md): Read iframe input-like element value.
- [`getAttribute`](getAttribute.md): Read iframe element attribute.
- [`frameGetAttribute`](frameGetAttribute.md): Read iframe element attribute.
- [`computedStyle`](computedStyle.md): Read iframe element computed CSS property.
- [`frameComputedStyle`](frameComputedStyle.md): Read iframe element computed CSS property.
- [`property`](property.md): Read iframe element JavaScript property.
- [`frameProperty`](frameProperty.md): Read iframe element JavaScript property.
- [`count`](count.md): Count iframe elements.
- [`frameCount`](frameCount.md): Count iframe elements.
- [`locatorCount`](locatorCount.md): Count iframe elements.
- [`frameLocatorCount`](frameLocatorCount.md): Count iframe elements.
- [`boundingBox`](boundingBox.md): Read iframe element bounding box.
- [`frameBoundingBox`](frameBoundingBox.md): Read iframe element bounding box.
- [`allTextContents`](allTextContents.md): Read iframe `textContent` values.
- [`frameAllTextContents`](frameAllTextContents.md): Read iframe `textContent` values.
- [`allInnerTexts`](allInnerTexts.md): Read iframe `innerText` values.
- [`frameAllInnerTexts`](frameAllInnerTexts.md): Read iframe `innerText` values.

## Behavior

- Requires a browser started with [`browser launch`](../../launch.md).
- The iframe must be same-origin and ready.
- The iframe selector itself is a top-page CSS selector.
- The element selector inside the iframe can be CSS or a CMG rich/provider locator such as `getByRole=button|Save`, `text=Saved`, `getByTestId=status`, `xpath=//button`, or `hasText=.row|Ready`.
- Runs the same underlying scripting actions as `browser control script`.
- Script GIF recordings move the virtual pointer to the top-page coordinate inside the iframe for frame click, hover, type, and fill actions. Frame getters are non-visual and do not move the pointer.
- Writes `PASS`, `FRAME`, `FRAME_EVALUATE`, or frame getter output lines to stdout.
- Writes browser, frame, selector, parse, or action errors to stderr.
- Exits `0` on success and `1` on failure.

## Examples

```powershell
cmg browser control frames waitForElement "#checkoutFrame" "#email" --timeout 5000
cmg browser control frames waitForSelector "#checkoutFrame" "#status" --timeout 5000
cmg browser control frames fill "#checkoutFrame" "#email" "agent@example.com"
cmg browser control frames click "#checkoutFrame" "#save"
cmg browser control frames click "#checkoutFrame" "getByRole=button|Save"
cmg browser control frames assertText "#checkoutFrame" "#status" "^Saved$" --match regex --ignore-case
cmg browser control frames toContainText "#checkoutFrame" "#status" "Saved"
cmg browser control frames computedStyle "#checkoutFrame" "#status" display
cmg browser control frames property "#checkoutFrame" "#status" dataset.state
```
