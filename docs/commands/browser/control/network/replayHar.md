# `browser control network replayHar`

Runs the scripting `replayHar` action once from the command line.

```powershell
cmg browser control network replayHar --path <path>
```

## Options

- `--path <path>`: Required HAR input path.

## Stdout

```text
PASS 001 replayHar
HAR_REPLAY 001 routes=1 C:\Projects\CMG\artifacts\network.har
```

## Exit Codes

- `0`: HAR routes were installed.
- `1`: Browser is not running, the HAR file was missing, or the action failed.
