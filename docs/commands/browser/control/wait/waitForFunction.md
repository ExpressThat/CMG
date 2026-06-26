# `browser control wait waitForFunction`

Runs the scripting `waitForFunction` action once from the command line.

```powershell
cmg browser control wait waitForFunction "<expression>" [--timeout <milliseconds>]
```

This is an exact-name alias for [`function`](function.md).

## Arguments

- `<expression>`: JavaScript expression that must become truthy.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `5000` when omitted.

## Stdout

```text
PASS 001 waitForFunction window.appReady
FUNCTION 001 true
```

## Stderr

Writes browser, JavaScript, timeout, or falsey-result errors.

## Exit Codes

- `0`: Expression became truthy before the timeout.
- `1`: Browser is not running, JavaScript failed, expression stayed falsey, or the action failed.

## Example

```powershell
cmg browser control wait waitForFunction "window.appReady === true" --timeout 10000
```
