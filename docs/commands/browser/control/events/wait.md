# `browser control events wait`

Waits for any event supported by the scripting `waitForEvent` action.

```powershell
cmg browser control events wait <event> [matcher] [options]
```

## Arguments

- `<event>`: Event name. Supported values include `popup`, `page`, `tab`, `download`, `dialog`, `console`, `pageError`, `request`, `requestFinished`, `requestFailed`, `response`, `websocket`, and `websocketMessage`.
- `[matcher]`: Optional event matcher text, such as dialog text, console text, URL text, or WebSocket text.

## Options

- `--timeout <milliseconds>`: Timeout in milliseconds.
- `--level <level>`: Console level filter.
- `--count <count>`: Expected tab or popup count.
- `--directory <path>`: Download directory.
- `--pattern <text>`: Download glob, URL matcher, or message matcher.
- `--method <method>`: HTTP method filter.
- `--status <status>`: HTTP status filter.
- `--contains <text>`: Body, response, or error text filter.
- `--mocked <true|false>`: Match mocked or real traffic.

## Stdout

Uses the mapped event action output, such as `DIALOG`, `CONSOLE`, `DOWNLOAD`, `REQUEST`, or `RESPONSE`.

## Stderr

Writes browser, event, matcher, timeout, parse, or action errors.

## Exit Codes

- `0`: A matching event was found.
- `1`: Browser is not running or the action failed.
