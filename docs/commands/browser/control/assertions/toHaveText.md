# `browser control assertions toHaveText`

Runs the scripting `toHaveText` action once from the command line.

```powershell
cmg browser control assertions toHaveText "<selector>" "<expected>" [--timeout <ms>] [--match <mode>] [--ignore-case]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.
- `<expected>`: Text fragment expected in the element's visible text.

## Options

- `--timeout <ms>`: Poll until the text matches or the timeout expires.
- `--match <mode>`: `contains`, `exact`, or `regex`. Default is `contains`.
- `--ignore-case`: Match text case-insensitively.

## Stdout

```text
PASS 001 toHaveText #status Ready
```

## Stderr

Writes browser, selector, timeout, or assertion failure errors.

## Exit Codes

- `0`: Element text contained the expected text.
- `1`: Browser is not running, no element matched, assertion failed, or the timeout expired.

## Example

```powershell
cmg browser control assertions toHaveText "#status" "Ready"
```
