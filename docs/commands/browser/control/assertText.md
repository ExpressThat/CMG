# `browser control assertText`

Runs the scripting `assertText` action once from the command line.

```powershell
cmg browser control assertText "<selector>" "<expected>"
```

## Arguments

- `<selector>`: CSS selector.
- `<expected>`: Text fragment expected in the element's visible text.

## Stdout

```text
PASS 001 assertText h1 "CMG Browser Control Test Page"
```

## Stderr

Writes assertion, browser, or missing-element errors.

## Exit Codes

- `0`: Element text contained the expected text.
- `1`: Browser is not running, no element matched, assertion failed, or the action failed.

## Example

```powershell
cmg browser control assertText "h1" "CMG Browser Control Test Page"
```
