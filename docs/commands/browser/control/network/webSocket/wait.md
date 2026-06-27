# `browser control network webSocket wait`

Runs the scripting `waitForWebSocket` action once from the command line.

```powershell
cmg browser control network webSocket wait "<pattern>" [options]
```

## Options

- `--timeout <milliseconds>`: Wait timeout. Default is `5000`.
- `--match <contains|exact|regex>`: Pattern match mode. Default is `contains`.
- `--ignore-case`: Match URL text without case sensitivity.

## Stdout

```text
PASS 001 waitForWebSocket /socket
WEBSOCKET 001 {"url":"/socket","routed":true}
```

## Failure

- Exit code `1` when the script cannot run, no matching WebSocket appears before timeout, the match mode is invalid, or `--match regex` receives an invalid regex pattern.

```text
FAIL 001 waitForWebSocket failed. Timed out waiting for websocket /socket
```

## Examples

```powershell
cmg browser control network webSocket wait "/socket/\d+" --match regex --ignore-case --timeout 10000
```
