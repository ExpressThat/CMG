# `browser control page runtime allTextContents`

Reads `textContent` for all matching elements as a JSON array.

```powershell
cmg browser control page runtime allTextContents "<selector>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Stdout

```text
PASS 001 allTextContents .item
TEXTS 001 ["One","Two"]
```

## Stderr

Writes browser, selector, parse, or action errors.

## Exit Codes

- `0`: Text values were read.
- `1`: Browser is not running or the action failed.
