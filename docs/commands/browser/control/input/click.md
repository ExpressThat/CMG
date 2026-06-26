# `browser control input click`

Runs the scripting `click` action once from the command line.

```powershell
cmg browser control input click "<selector>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Stdout

```text
PASS 001 click #save
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: The element was clicked.
- `1`: Browser is not running or the action failed.
