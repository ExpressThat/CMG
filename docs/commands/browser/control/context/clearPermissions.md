# `browser control context clearPermissions`

Clears page-side permission grants.

```powershell
cmg browser control context clearPermissions
```

## Stdout

```text
PASS 001 clearPermissions
PERMISSIONS_CLEARED 001
```

## Exit Codes

- `0`: Permissions were cleared.
- `1`: Browser is not running or the action failed.
