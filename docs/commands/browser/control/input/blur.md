# `browser control input blur`

Runs the scripting `blur` action once from the command line.

```powershell
cmg browser control input blur "<selector>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Stdout

```text
PASS 001 blur #name
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: The element was blurred.
- `1`: Browser is not running or the action failed.
