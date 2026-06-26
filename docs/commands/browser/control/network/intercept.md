# `browser control network intercept`

Runs the scripting `intercept` action once from the command line. This is a route alias for users who think in Cypress-style interception terms.

```powershell
cmg browser control network intercept "<pattern>" [options]
```

## Arguments

- `<pattern>`: URL substring to match.

## Options

- `--status <status>`: Mocked response status.
- `--body <body>`: Mocked response body.
- `--content-type <type>`: Mocked response content type.
- `--method <method>`: HTTP method filter.
- `--times <count>`: Remove route after this many matches.
- `--delay <milliseconds>`: Response delay.
- `--abort`: Abort matching requests.

## Stdout

```text
PASS 001 intercept /api/profile
ROUTE 001 /api/profile
```

## Stderr

Writes browser, option, or action errors.

## Exit Codes

- `0`: Intercept route was installed.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control network intercept "/api/profile" --method GET --status 200 --body "{\"name\":\"CMG\"}"
cmg browser control network intercept "/api/private" --abort
```
