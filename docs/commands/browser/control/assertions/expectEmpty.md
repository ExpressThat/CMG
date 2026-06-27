# `browser control assertions expectEmpty`

Runs the scripting `expectEmpty` action once from the command line.

```powershell
cmg browser control assertions expectEmpty "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 expectEmpty #name
EXPECT 001 empty #name
```

## Stderr

Writes browser, selector, timeout, empty-state, or action errors.

## Exit Codes

- `0`: Element text/value was empty.
- `1`: Browser is not running, no element matched, element was not empty, or the action failed.

## Example

```powershell
cmg browser control assertions expectEmpty "#name"
```
