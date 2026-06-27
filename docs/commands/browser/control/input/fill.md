# `browser control input fill`

Runs the scripting `fill` action once from the command line.

```powershell
cmg browser control input fill "<selector>" "<text>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.
- `<text>`: Text to set as the element value.

## Stdout

```text
PASS 001 fill #name "CMG Test"
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: The element value was replaced.
- `1`: Browser is not running or the action failed.
