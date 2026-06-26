# `browser control input select`

Runs the scripting `select` action once from the command line.

```powershell
cmg browser control input select "<selector>" "<value>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.
- `<value>`: Value to select.

## Stdout

```text
PASS 001 select #country GB
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: The select-like element value was changed.
- `1`: Browser is not running or the action failed.
