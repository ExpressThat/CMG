# `browser control network webSocket wait`

Runs the scripting `waitForWebSocket` action once from the command line.

```powershell
cmg browser control network webSocket wait "<pattern>" [--timeout <milliseconds>]
```

## Stdout

```text
PASS 001 waitForWebSocket /socket
WEBSOCKET 001 {"url":"/socket","routed":true}
```
