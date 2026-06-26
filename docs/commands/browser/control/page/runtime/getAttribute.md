# `browser control page runtime getAttribute`

Reads an element attribute.

```powershell
cmg browser control page runtime getAttribute "<selector>" "<name>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.
- `<name>`: Attribute name.

## Stdout

```text
PASS 001 getAttribute #profile href
ATTRIBUTE 001 /profile
```

## Exit Codes

- `0`: Attribute was read.
- `1`: Browser is not running or the action failed.
