# `browser control input uncheck`

Runs the scripting `uncheck` action once from the command line.

```powershell
cmg browser control input uncheck "<selector>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Stdout

```text
PASS 001 uncheck #subscribe
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: The element was unchecked.
- `1`: Browser is not running or the action failed.
