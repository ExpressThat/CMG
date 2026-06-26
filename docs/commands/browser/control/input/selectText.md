# `browser control input selectText`

Runs the scripting `selectText` action once from the command line.

```powershell
cmg browser control input selectText "<selector>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Stdout

```text
PASS 001 selectText #name
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: Text selection was requested on the element.
- `1`: Browser is not running or the action failed.
