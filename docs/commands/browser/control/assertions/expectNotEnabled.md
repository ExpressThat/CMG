# `browser control assertions expectNotEnabled`

Runs the scripting `expectNotEnabled` action once from the command line.

```powershell
cmg browser control assertions expectNotEnabled "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 expectNotEnabled #archive
EXPECT 001 disabled #archive
```

## Stderr

Writes browser, selector, timeout, enabled-state mismatch, or action errors.

## Exit Codes

- `0`: Element was disabled or aria-disabled.
- `1`: Browser is not running, no element matched, element stayed enabled, or the action failed.

## Example

```powershell
cmg browser control assertions expectNotEnabled "#archive"
```
