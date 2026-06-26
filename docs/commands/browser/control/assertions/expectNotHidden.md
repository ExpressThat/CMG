# `browser control assertions expectNotHidden`

Runs the scripting `expectNotHidden` action once from the command line.

```powershell
cmg browser control assertions expectNotHidden "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. The scripting default is `0` when omitted.

## Stdout

```text
PASS 001 expectNotHidden #save
EXPECT 001 visible #save
```

## Stderr

Writes browser, selector, timeout, visibility, or action errors.

## Exit Codes

- `0`: Element was visible.
- `1`: Browser is not running, no element matched, element stayed hidden, or the action failed.

## Example

```powershell
cmg browser control assertions expectNotHidden "#save" --timeout 5000
```
