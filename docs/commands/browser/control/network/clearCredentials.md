# `browser control network clearCredentials`

Runs the scripting `clearHttpCredentials` action once from the command line.

```powershell
cmg browser control network clearCredentials
```

## Stdout

```text
PASS 001 clearHttpCredentials
HTTP_CREDENTIALS_CLEARED 001
```

## Exit Codes

- `0`: HTTP credentials were cleared.
- `1`: Browser is not running or the action failed.
