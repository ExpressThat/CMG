# `browser control assertions assertVisible`

Runs the scripting `assertVisible` action once from the command line.

```powershell
cmg browser control assertions assertVisible "<selector>" [--timeout <milliseconds>]
```

Unlike [`expectVisible`](expectVisible.md), this exact script alias uses the wait-for-element path and emits only the standard `PASS` line when it succeeds.

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `5000` when omitted.

## Stdout

```text
PASS 001 assertVisible #save
```

## Stderr

Writes browser, selector, timeout, visibility, or action errors.

## Exit Codes

- `0`: Element existed and was visible before the timeout.
- `1`: Browser is not running, no element matched, element was not visible, or the action failed.

## Example

```powershell
cmg browser control assertions assertVisible "#save" --timeout 5000
```
