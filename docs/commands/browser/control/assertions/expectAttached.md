# `browser control assertions expectAttached`

Runs the scripting `expectAttached` action once from the command line.

```powershell
cmg browser control assertions expectAttached "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 expectAttached #save
EXPECT 001 attached #save
```

## Stderr

Writes browser, selector, timeout, attached-state, or action errors.

## Exit Codes

- `0`: Element was attached.
- `1`: Browser is not running, no element matched, element was detached, or the action failed.

## Example

```powershell
cmg browser control assertions expectAttached "#save"
```
