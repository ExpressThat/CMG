# `browser control input type`

Runs the scripting `type` action once from the command line.

```powershell
cmg browser control input type "<selector>" "<text>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.
- `<text>`: Text to append to the element value.

## Stdout

```text
PASS 001 type #name "CMG Test"
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: Text was typed into the element.
- `1`: Browser is not running or the action failed.
