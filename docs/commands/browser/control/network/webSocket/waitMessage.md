# `browser control network webSocket waitMessage`

Runs the scripting `waitForWebSocketMessage` action once from the command line.

```powershell
cmg browser control network webSocket waitMessage "<pattern>" [options]
```

## Options

- `--timeout <milliseconds>`: Wait timeout. Default is `5000`.
- `--match <contains|exact|regex>`: Pattern match mode. Default is `contains`.
- `--ignore-case`: Match message data or URL text without case sensitivity.

## Stdout

```text
PASS 001 waitForWebSocketMessage ready
WEBSOCKET_MESSAGE 001 {"url":"/socket","data":"ready","routed":true}
```

## Failure

- Exit code `1` when the script cannot run, no matching WebSocket message appears before timeout, the match mode is invalid, or `--match regex` receives an invalid regex pattern.

```text
FAIL 001 waitForWebSocketMessage failed. Timed out waiting for websocket message ready
```

## Examples

```powershell
cmg browser control network webSocket waitMessage "^ready:" --match regex --ignore-case
```
