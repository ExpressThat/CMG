# `browser control context grantPermissions`

Grants page-side permissions.

```powershell
cmg browser control context grantPermissions <permission> [permission...]
```

## Arguments

- `<permission>`: Permission name to report as granted.

## Stdout

```text
PASS 001 grantPermissions geolocation notifications
PERMISSIONS 001 geolocation,notifications
```

## Exit Codes

- `0`: Permissions were granted.
- `1`: Browser is not running or the action failed.
