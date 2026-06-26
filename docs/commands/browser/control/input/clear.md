# `browser control input clear`

Runs the scripting `clear` action once from the command line.

```powershell
cmg browser control input clear "<selector>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Stdout

```text
PASS 001 clear #name
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: The element value was cleared.
- `1`: Browser is not running or the action failed.
