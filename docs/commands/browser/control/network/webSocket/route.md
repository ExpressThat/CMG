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

## Stdout

```text
PASS 001 routeWebSocket /socket
WEBSOCKET_ROUTE 001 /socket
```
