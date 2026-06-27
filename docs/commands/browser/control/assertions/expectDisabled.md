# `browser control assertions expectDisabled`

Runs the scripting `expectDisabled` action once from the command line.

```powershell
cmg browser control assertions expectDisabled "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `5000` when omitted.

## Stdout

```text
PASS 001 expectDisabled #save
EXPECT 001 disabled #save
```

## Stderr

Writes browser, selector, timeout, disabled-state, or action errors.

## Exit Codes

- `0`: Element became disabled before the timeout.
- `1`: Browser is not running, no element matched, element was enabled, or the action failed.

## Example

```powershell
cmg browser control assertions expectDisabled "#save"
```
