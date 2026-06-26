# `browser control network route`

Runs the scripting `route` action once from the command line.

```powershell
cmg browser control network route "<pattern>" [options]
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
PASS 001 route /api/profile
ROUTE 001 /api/profile
```

## Stderr

Writes browser, option, or action errors.

## Exit Codes

- `0`: Route was installed.
- `1`: Browser is not running or the action failed.
