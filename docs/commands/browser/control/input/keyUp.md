# `browser control input keyUp`

Runs the scripting `keyUp` action once from the command line.

```powershell
cmg browser control input keyUp "<key>"
```

## Arguments

- `<key>`: Key name, such as `Shift` or `Enter`.

## Stdout

```text
PASS 001 keyUp Shift
KEY_UP 001 Shift
```

## Stderr

Writes browser or keyboard dispatch errors.

## Exit Codes

- `0`: The keyup event was dispatched.
- `1`: Browser is not running or the action failed.
