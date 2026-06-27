# `browser close`

Closes the selected CMG-controlled browser instance. Chrome is the default. Use `--chrome` to select Chrome explicitly, `--edge` to close Edge, or `--firefox` to close Firefox.

```powershell
cmg browser close
cmg browser --port <port> close
cmg --chrome browser close
cmg --edge browser close
cmg --firefox browser close
```

## Browser Group Options

- `browser --port <port>`: Remote debugging port for the browser instance to close. Defaults to Chrome `9222`, Edge `9224`, and Firefox `9223`.

## Behavior

- Reads the selected browser and port state file under `%LOCALAPPDATA%\CMG`.
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
- `1`: The selected browser could not be closed, or `browser --port` is outside `1..65535`.

## Example

```powershell
cmg browser close
cmg browser --port 9333 close
cmg --edge browser close
cmg --firefox browser close
```
