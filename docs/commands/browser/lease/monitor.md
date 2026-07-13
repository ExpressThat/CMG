# `browser lease monitor` (internal)

Hidden internal command started by CMG after an opt-in leased launch. Agents and people should use [`status`](status.md), [`keepAlive`](keepAlive.md), or [`disable`](disable.md), not this command.

```powershell
cmg browser --port <port> lease monitor --token <ownership-token>
```

## Options

- `--token <ownership-token>`: Required CMG-generated launch token. It prevents an old monitor from affecting a replacement browser.

## Output

The monitor persists parseable `BROWSER_IDLE_CLEANUP` events for warning, skipped, closed, and failed outcomes. `lease status` can expose the final event after browser state is cleared.

## Exit Codes

- `0`: The lease was disabled/replaced/missing, or the owned expired browser closed successfully.
- `1`: Closing the still-owned expired browser failed.
