# `browser lease disable`

Disables idle cleanup for the selected browser without closing it.

```powershell
cmg browser --port <port> lease disable
```

## Arguments And Options

This command has no leaf arguments or options. Use `browser --port <port>` and the global browser selector to choose the instance.

## Stdout

```text
BROWSER_IDLE_LEASE status=disabled browser=<browser> port=<port> pid=<pid> ownership=cmg idleTimeoutMs=0 deadline=none reason=caller-request
```

## Stderr

A missing browser writes a precise error naming the selected browser and port.

## Exit Codes

- `0`: Cleanup was disabled.
- `1`: No selected CMG browser state exists.

## Example

```powershell
cmg browser --port 9333 lease disable
```
