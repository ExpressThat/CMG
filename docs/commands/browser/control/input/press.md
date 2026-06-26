# `browser control input press`

Runs the scripting `press` action once from the command line.

```powershell
cmg browser control input press "<key-or-chord>"
```

## Arguments

- `<key-or-chord>`: Key name such as `Enter`, or a chord such as `Control+A`.

## Stdout

```text
PASS 001 press Enter
```

When `<key-or-chord>` contains `+`, `press` runs shortcut behavior and also emits:

```text
KEYBOARD_SHORTCUT 001 Control+A
```

## Stderr

Writes browser or keyboard errors.

## Exit Codes

- `0`: The key press was dispatched.
- `1`: Browser is not running, the chord is invalid, or the action failed.

## Examples

```powershell
cmg browser control input press Enter
cmg browser control input press Control+A
```
