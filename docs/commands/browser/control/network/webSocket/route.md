# `browser control network webSocket route`

Runs the scripting `routeWebSocket` action once from the command line.

```powershell
cmg browser control network webSocket route "<pattern>" [options]
```

## Options

- `--message <text>`: Message to send after open.
- `--close <true|false>`: Whether to close the socket.
- `--code <code>`: WebSocket close code.
- `--reason <text>`: WebSocket close reason.
- `--match <contains|exact|regex>`: Pattern match mode. Default is `contains`.
- `--ignore-case`: Match URL text without case sensitivity.

## Stdout

```text
PASS 001 routeWebSocket /socket
WEBSOCKET_ROUTE 001 /socket
```

## Failure

- Exit code `1` when the script cannot run, the match mode is invalid, or `--match regex` receives an invalid regex pattern.

```text
FAIL 001 routeWebSocket failed. Invalid network regex '[': ...
```

## Examples

```powershell
cmg browser control network webSocket route "/socket/\d+" --match regex --ignore-case --message ready
```
