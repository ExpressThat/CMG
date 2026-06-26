# `browser control assertions assertText`

Runs the scripting `assertText` action once from the command line.

```powershell
cmg browser control assertions assertText "<selector>" "<expected>" [--timeout <ms>]
```

## Arguments

- `<selector>`: CSS selector.
- `<expected>`: Text fragment expected in the element's visible text.

## Options

- `--timeout <ms>`: Poll until the element text contains the expected text or the timeout expires.

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
cmg browser control assertions assertText "h1" "CMG Browser Control Test Page" --timeout 5000
```
