# `browser control assertions expectNotDisabled`

Runs the scripting `expectNotDisabled` action once from the command line.

```powershell
cmg browser control assertions expectNotDisabled "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 expectNotDisabled #save
EXPECT 001 enabled #save
```

## Stderr

Writes browser, selector, timeout, disabled-state mismatch, or action errors.

## Exit Codes

- `0`: Element was enabled.
- `1`: Browser is not running, no element matched, element stayed disabled, or the action failed.

## Example

```powershell
cmg browser control assertions expectNotDisabled "#save"
```
