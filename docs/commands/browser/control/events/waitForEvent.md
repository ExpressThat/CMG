# `browser control events waitForEvent`

Runs the scripting `waitForEvent` action once from the command line.

```powershell
cmg browser control events waitForEvent <event> [matcher] [options]
```

## Arguments

- `<event>`: Event name, such as `dialog`, `console`, `request`, `response`, or `download`.
- `[matcher]`: Optional event matcher text.

## Options

- `--timeout <milliseconds>`: Timeout in milliseconds.
- `--level <level>`: Console level filter.
- `--count <count>`: Expected tab or popup count.
- `--directory <directory>`: Download directory.
- `--pattern <pattern>`: Download file glob or URL/message matcher.
- `--method <method>`: HTTP method filter.
- `--status <status>`: HTTP status filter.
- `--contains <text>`: Body, response, or error text filter.
- `--mocked <true|false>`: Whether to match mocked or real network traffic.

## Stdout

```text
PASS 001 waitForEvent response /api
RESPONSE 001 /api 200
```

## Stderr

Writes browser, argument, timeout, parse, or action errors.

## Exit Codes

- `0`: Matching event was observed.
- `1`: Browser is not running, arguments are invalid, or the wait timed out.

## Examples

```powershell
cmg browser control events waitForEvent response "/api/profile" --status 200 --timeout 5000
```
