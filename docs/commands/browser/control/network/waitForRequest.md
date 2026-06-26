# `browser control network waitForRequest`

Runs the scripting `waitForRequest` action once from the command line.

```powershell
cmg browser control network waitForRequest "<pattern>" [options]
```

## Arguments

- `<pattern>`: URL substring to match.

## Options

- `--timeout <milliseconds>`: Timeout.
- `--method <method>`: HTTP method filter.
- `--status <status>`: HTTP status filter.
- `--contains <text>`: Body, response, or error text filter.
- `--mocked <true|false>`: Match mocked or real traffic.

## Stdout

```text
PASS 001 waitForRequest /api/profile
REQUEST 001 {"method":"GET","url":"/api/profile"}
```

## Stderr

Writes browser, timeout, option, or action errors.

## Exit Codes

- `0`: A matching request was found.
- `1`: Browser is not running or the action failed.
