# `browser control network clearHttpCredentials`

Runs the scripting `clearHttpCredentials` action once from the command line.

```powershell
cmg browser control network clearHttpCredentials
```

## Stdout

```text
PASS 001 clearHttpCredentials
CREDENTIALS_CLEARED 001
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: HTTP credentials were cleared.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control network clearHttpCredentials
```
