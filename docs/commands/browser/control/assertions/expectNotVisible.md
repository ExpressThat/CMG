# `browser control assertions expectNotVisible`

Runs the scripting `expectNotVisible` action once from the command line.

```powershell
cmg browser control assertions expectNotVisible "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 expectNotVisible #spinner
EXPECT 001 hidden #spinner
```

## Stderr

Writes browser, selector, timeout, visibility, or action errors.

## Exit Codes

- `0`: Element was hidden, detached, missing, or not visible.
- `1`: Browser is not running, element stayed visible, or the action failed.

## Example

```powershell
cmg browser control assertions expectNotVisible "#spinner" --timeout 5000
```
