# `browser control network authenticate`

Runs the scripting `authenticate` alias once from the command line. It maps to the same behavior as `setHttpCredentials`.

```powershell
cmg browser control network authenticate <username> <password>
```

## Arguments

- `<username>`: HTTP auth username.
- `<password>`: HTTP auth password.

## Stdout

```text
PASS 001 setHttpCredentials user pass
CREDENTIALS_SET 001 user
```

## Stderr

Writes browser, argument, or action errors.

## Exit Codes

- `0`: Credentials were set.
- `1`: Browser is not running, arguments are invalid, or the action failed.

## Examples

```powershell
cmg browser control network authenticate user pass
```
