# `browser control input keyDown`

Runs the scripting `keyDown` action once from the command line.

```powershell
cmg browser control input keyDown "<key>"
```

## Arguments

- `<key>`: Key name, such as `Shift` or `Enter`.

## Stdout

```text
PASS 001 keyDown Shift
KEY_DOWN 001 Shift
```

## Stderr

Writes browser or keyboard dispatch errors.

## Exit Codes

- `0`: The keydown event was dispatched.
- `1`: Browser is not running or the action failed.
