# `browser control network setOffline`

Runs the scripting `setOffline` action once from the command line.

```powershell
cmg browser control network setOffline <enabled>
```

## Arguments

- `<enabled>`: `true` to simulate offline, `false` to restore online behavior.

## Stdout

```text
PASS 001 setOffline true
OFFLINE 001 true
```

## Exit Codes

- `0`: Offline simulation was updated.
- `1`: Browser is not running or the action failed.
