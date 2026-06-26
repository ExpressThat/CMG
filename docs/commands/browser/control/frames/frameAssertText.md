# `browser control frames frameAssertText`

Runs the scripting `frameAssertText` action once from the command line.

```powershell
cmg browser control frames frameAssertText "<frameSelector>" "<selector>" "<text>"
```

## Arguments

- `<frameSelector>`: CSS selector for the same-origin iframe.
- `<selector>`: CSS selector inside the iframe.
- `<text>`: Expected text.

## Stdout

```text
PASS 001 frameAssertText #frame #status Saved
FRAME 001 frameAssertText
```

## Stderr

Writes browser, frame, selector, assertion, parse, or action errors.

## Exit Codes

- `0`: Text matched.
- `1`: Browser is not running, the frame is missing, or the assertion failed.

## Examples

```powershell
cmg browser control frames frameAssertText "#checkoutFrame" "#status" "Saved"
```
