# `browser control page runtime count`

Counts matching elements.

```powershell
cmg browser control page runtime count "<selector>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Stdout

```text
PASS 001 count .row
COUNT 001 3
```

## Stderr

Writes browser, selector, parse, or action errors.

## Exit Codes

- `0`: Count was read.
- `1`: Browser is not running or the action failed.
