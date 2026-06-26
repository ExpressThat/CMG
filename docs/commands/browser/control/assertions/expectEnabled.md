# `browser control assertions expectEnabled`

Runs the scripting `expectEnabled` action once from the command line.

```powershell
cmg browser control assertions expectEnabled "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `5000` when omitted.

## Stdout

```text
PASS 001 expectEnabled #save
EXPECT 001 enabled #save
```

## Stderr

Writes browser, selector, timeout, enabled-state, or action errors.

## Exit Codes

- `0`: Element became enabled before the timeout.
- `1`: Browser is not running, no element matched, element was disabled, or the action failed.

## Example

```powershell
cmg browser control assertions expectEnabled "#save"
```
