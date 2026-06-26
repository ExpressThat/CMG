# `browser control input press`

Runs the scripting `press` action once from the command line.

```powershell
cmg browser control input press "<key>"
```

## Arguments

- `<key>`: Key name, such as `Enter` or `Escape`.

## Stdout

```text
PASS 001 press Enter
```

## Stderr

Writes browser or keyboard errors.

## Exit Codes

- `0`: The key press was dispatched.
- `1`: Browser is not running or the action failed.
