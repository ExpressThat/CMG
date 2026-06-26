# `browser control network setCredentials`

Runs the scripting `setHttpCredentials` action once from the command line.

```powershell
cmg browser control network setCredentials <username> <password>
```

## Arguments

- `<username>`: HTTP auth username.
- `<password>`: HTTP auth password.

## Stdout

```text
PASS 001 setHttpCredentials agent secret
HTTP_CREDENTIALS_SET 001 agent
```

## Exit Codes

- `0`: Credentials were installed for current and future page requests.
- `1`: Browser is not running or the action failed.
