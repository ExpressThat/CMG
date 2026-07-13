# `browser lease keepAlive`

Renews the selected headless browser's idle lease. Use this before long non-browser work when an agent expects to return later.

```powershell
cmg browser --port <port> lease keepAlive [--idle-timeout <duration>]
```

## Options

- `--idle-timeout <duration>`: Optional replacement positive duration using `ms`, `s`, `m`, or `h`, such as `45m` or `2h`. When omitted, CMG keeps the existing duration.

## Stdout

```text
BROWSER_IDLE_LEASE status=renewed browser=<browser> port=<port> pid=<pid> ownership=cmg idleTimeoutMs=<milliseconds> deadline=<ISO-8601> reason=keepalive
```

## Stderr

Failures explain whether the browser is missing, unowned/not headless, has no active lease, or the duration is invalid.

## Exit Codes

- `0`: The lease was renewed.
- `1`: Renewal was unsafe or invalid.

## Examples

```powershell
cmg browser --port 9333 lease keepAlive
cmg browser --port 9333 lease keepAlive --idle-timeout 2h
```
