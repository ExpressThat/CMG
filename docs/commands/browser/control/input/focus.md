# `browser control input focus`

Runs the scripting `focus` action once from the command line.

```powershell
cmg browser control input focus "<selector>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Stdout

```text
PASS 001 focus #name
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: The element was focused.
- `1`: Browser is not running or the action failed.
