# `browser lease`

Inspects and manages conservative idle cleanup for a selected CMG-owned headless browser.

```powershell
cmg browser --port <port> lease <command>
```

Idle cleanup is disabled by default. Enable it with `browser launch --headless --idle-timeout <duration>`. A lease is a renewable deadline, not a command timeout: CMG refreshes it whenever the selected browser is controlled and throughout long scripts or test runs. Normal agent work between browser commands should fit comfortably inside the chosen duration.

CMG never leases visible browsers, attached application/browser endpoints, user-launched processes, or state whose ownership token/process identity no longer matches. At expiry, the monitor writes a warning, rechecks for renewed activity, then gracefully closes only a still-expired owned process; forced tree cleanup is a bounded fallback.

## Commands

- [`status`](status.md): Show the active/disabled/missing lease state and deadline.
- [`keepAlive`](keepAlive.md): Renew the lease and optionally replace its duration.
- [`disable`](disable.md): Disable cleanup without closing the browser.
- [`monitor`](monitor.md): Internal hidden monitor command; agent callers should not invoke it.

## Browser Group Options

- `browser --port <port>`: Selects the independent browser instance. Defaults to Chrome `9222`, Edge `9224`, or Firefox `9223`.

## Examples

```powershell
cmg browser --port 9333 launch --headless --idle-timeout 45m
cmg browser --port 9333 lease status
cmg browser --port 9333 lease keepAlive --idle-timeout 2h
cmg browser --port 9333 lease disable
```
