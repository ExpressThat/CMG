# `browser control network clearRoutes`

Runs the scripting `clearRoutes` action once from the command line.

```powershell
cmg browser control network clearRoutes
```

## Stdout

```text
PASS 001 clearRoutes
ROUTES_CLEARED 001
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Routes were cleared.
- `1`: Browser is not running or the action failed.
