# `browser control page runtime property`

Reads a JavaScript property from an element.

```powershell
cmg browser control page runtime property "<selector>" "<name>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.
- `<name>`: Dot-separated JavaScript property path, such as `dataset.state`, `checked`, or `ariaLabel`.

## Stdout

```text
PASS 001 property #status dataset.state
PROPERTY 001 ready
```

## Stderr

Writes browser, selector, parse, or action errors. Missing elements include the selector that failed.

## Exit Codes

- `0`: Property was read.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control page runtime property "#status" dataset.state
cmg browser control page runtime property "getByLabel=Remember me" checked
```
