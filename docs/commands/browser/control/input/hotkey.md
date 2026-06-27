# `browser control input hotkey`

Alias for [`keyboardShortcut`](keyboardShortcut.md).

```powershell
cmg browser control input hotkey "<chord>"
```

## Arguments

- `<chord>`: Keyboard chord such as `Control+S`, `Control+Shift+P`, or `Meta+K`.

## Options

None.

## Stdout

```text
PASS 001 keyboardShortcut Meta+K
KEYBOARD_SHORTCUT 001 Meta+K
```

## Stderr

Writes browser, keyboard dispatch, parse, or chord validation errors.

## Exit Codes

- `0`: The shortcut chord was dispatched.
- `1`: Browser is not running, the chord is invalid, or dispatch failed.

## Examples

```powershell
cmg browser control input hotkey Meta+K
```
