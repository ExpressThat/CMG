# `browser control frames frameClick`

Runs the scripting `frameClick` action once from the command line.

```powershell
cmg browser control frames frameClick "<frameSelector>" "<selector>"
```

## Arguments

- `<frameSelector>`: CSS selector for the same-origin iframe.
- `<selector>`: CSS selector or CMG rich/provider locator inside the iframe.

## Stdout

```text
PASS 001 frameClick #frame #save
FRAME 001 frameClick
```

## Stderr

Writes browser, frame, selector, parse, or action errors.

## Exit Codes

- `0`: Element was clicked.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control frames frameClick "#checkoutFrame" "#save"
```
