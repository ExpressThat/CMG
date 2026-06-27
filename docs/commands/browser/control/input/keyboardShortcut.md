# `browser control input keyboardShortcut`

Runs the scripting `keyboardShortcut` action once from the command line.

```powershell
cmg browser control input keyboardShortcut "<chord>"
```

## Arguments

- `<chord>`: Keyboard chord such as `Control+S`, `Control+Shift+P`, or `Meta+K`.

## Stdout

```text
PASS 001 keyboardShortcut Control+S
KEYBOARD_SHORTCUT 001 Control+S
```

## Stderr

Writes browser, keyboard dispatch, parse, or chord validation errors.

## Exit Codes

- `0`: The shortcut chord was dispatched.
- `1`: Browser is not running, the chord is invalid, or dispatch failed.

## Examples

```powershell
cmg browser control input keyboardShortcut Control+Shift+P
```
