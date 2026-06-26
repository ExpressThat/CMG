# `browser control page runtime boundingBox`

Reads an element bounding box as JSON.

```powershell
cmg browser control page runtime boundingBox "<selector>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Stdout

```text
PASS 001 boundingBox #card
BOUNDING_BOX 001 {"x":10,"y":20,"width":120,"height":40}
```

## Stderr

Writes browser, selector, parse, or action errors.

## Exit Codes

- `0`: Bounding box was read.
- `1`: Browser is not running, no element matched, or the action failed.
