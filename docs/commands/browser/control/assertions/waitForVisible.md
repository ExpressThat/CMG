# `browser control assertions waitForVisible`

Runs the scripting `waitForVisible` action once from the command line.

```powershell
cmg browser control assertions waitForVisible "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `5000` when omitted.

## Stdout

```text
PASS 001 waitForVisible #save
EXPECT 001 visible #save
```

## Stderr

Writes browser, selector, timeout, visibility, or action errors.

## Exit Codes

- `0`: Element became visible before the timeout.
- `1`: Browser is not running, no element matched, element was not visible, or the action failed.

## Example

```powershell
cmg browser control assertions waitForVisible "#save" --timeout 5000
```
