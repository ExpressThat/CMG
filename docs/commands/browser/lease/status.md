# `browser lease status`

Shows the selected browser's current idle-cleanup state without renewing it.

```powershell
cmg browser --port <port> lease status
```

## Arguments And Options

This command has no leaf arguments or options. Use `browser --port <port>` and the global browser selector to choose the instance.

## Stdout

```text
BROWSER_IDLE_LEASE status=<active|disabled|missing> browser=<chrome|edge|firefox> port=<port> pid=<pid> ownership=cmg idleTimeoutMs=<milliseconds> deadline=<ISO-8601|none> reason=status
```

If cleanup already occurred and state was cleared, CMG may return the final persisted `BROWSER_IDLE_CLEANUP ...` event instead of `status=missing`.

## Stderr

No stderr is written for an ordinary missing or disabled lease.

## Exit Codes

- `0`: Status was reported, including missing/disabled state.

## Example

```powershell
cmg browser --port 9333 lease status
```
