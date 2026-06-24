# `browser close`

Closes the selected CMG-controlled browser instance. Chrome is the default. Use `--chrome` to select Chrome explicitly, `--edge` to close Edge, or `--firefox` to close Firefox.

```powershell
cmg browser close
cmg --chrome browser close
cmg --edge browser close
cmg --firefox browser close
```

## Behavior

- Reads the selected browser state file under `%LOCALAPPDATA%\CMG`.
- Closes the tracked selected browser process.
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

- `0`: No browser was running, stale state was cleared, or the selected browser was closed successfully.
- `1`: The selected browser could not be closed.

## Example

```powershell
cmg browser close
cmg --edge browser close
cmg --firefox browser close
```
