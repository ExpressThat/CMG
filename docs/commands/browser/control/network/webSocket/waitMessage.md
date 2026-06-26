# `browser control network webSocket waitMessage`

Runs the scripting `waitForWebSocketMessage` action once from the command line.

```powershell
cmg browser control network webSocket waitMessage "<pattern>" [--timeout <milliseconds>]
```

## Stdout

```text
PASS 001 waitForWebSocketMessage ready
WEBSOCKET_MESSAGE 001 {"url":"/socket","data":"ready","routed":true}
```
