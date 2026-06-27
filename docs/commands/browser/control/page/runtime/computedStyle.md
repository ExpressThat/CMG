# `browser control page runtime computedStyle`

Reads a computed CSS property from an element.

```powershell
cmg browser control page runtime computedStyle "<selector>" "<property>"
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.
- `<property>`: CSS property name, such as `display`, `color`, or `background-color`.

## Stdout

```text
PASS 001 computedStyle #status display
STYLE 001 block
```

## Stderr

Writes browser, selector, parse, or action errors. Missing elements include the selector that failed.

## Exit Codes

- `0`: Computed style was read.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control page runtime computedStyle "#status" display
cmg browser control page runtime computedStyle "getByRole=button|Save" background-color
```
