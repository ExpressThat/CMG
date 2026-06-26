# `browser control network mockResponse`

Runs the scripting `mockResponse` action once from the command line. This is a route alias for explicitly mocked fetch/XHR responses.

```powershell
cmg browser control network mockResponse "<pattern>" [options]
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
PASS 001 mockResponse /api/profile
ROUTE 001 /api/profile
```

## Stderr

Writes browser, option, or action errors.

## Exit Codes

- `0`: Mock response route was installed.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control network mockResponse "/api/profile" --status 200 --body "{\"name\":\"CMG\"}" --content-type application/json
cmg browser control network mockResponse "/api/slow" --status 200 --delay 250
```
