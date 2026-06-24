# `browser close`

Closes the CMG-controlled Chrome instance.

```powershell
cmg browser close
```

## Behavior

- Reads `%LOCALAPPDATA%\CMG\browser.state`.
- Closes the tracked Chrome process.
- Clears stale browser state when the tracked process no longer exists.
- Ignores extra arguments and prints them as ignored.

## Stdout

When a browser is closed:

```text
Closed CMG-controlled Chrome. PID: <pid>.
```

When no tracked browser is running:

```text
No CMG-controlled Chrome instance is running.
```

## Exit Codes

- `0`: No browser was running, stale state was cleared, or Chrome was closed successfully.
- `1`: Chrome could not be closed.

## Example

```powershell
cmg browser close
```
