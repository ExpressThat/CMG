# `browser control frames frameEvaluate`

Runs the scripting `frameEvaluate` action once from the command line.

```powershell
cmg browser control frames frameEvaluate "<frameSelector>" "<expression>"
```

## Arguments

- `<frameSelector>`: CSS selector for the same-origin iframe.
- `<expression>`: JavaScript expression evaluated in the iframe.

## Stdout

```text
PASS 001 frameEvaluate #frame document.title
FRAME_EVALUATE 001 Checkout
```

## Stderr

Writes browser, frame, JavaScript, parse, or action errors.

## Exit Codes

- `0`: Expression was evaluated.
- `1`: Browser is not running, the frame is missing, or evaluation failed.

## Examples

```powershell
cmg browser control frames frameEvaluate "#checkoutFrame" "document.title"
```
