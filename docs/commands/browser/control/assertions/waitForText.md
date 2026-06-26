# `browser control assertions waitForText`

Runs the scripting `waitForText` action once from the command line.

```powershell
cmg browser control assertions waitForText "<selector>" "<expected>" [--timeout <ms>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<expected>`: Text fragment expected in the element's visible text.

## Options

- `--timeout <ms>`: Poll until the text matches or the timeout expires.

## Stdout

```text
PASS 001 waitForText #status Ready
```

## Stderr

Writes browser, selector, timeout, or assertion failure errors.

## Exit Codes

- `0`: Element text contained the expected text before the timeout.
- `1`: Browser is not running, no element matched, assertion failed, or the timeout expired.

## Example

```powershell
cmg browser control assertions waitForText "#status" "Ready" --timeout 5000
```
