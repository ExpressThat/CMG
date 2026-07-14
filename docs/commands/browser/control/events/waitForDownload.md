# `browser control events waitForDownload`

Waits for a matching downloaded file without clicking first.

The command snapshots existing matches when it starts, then waits for a new or changed file that is stable across two polls. It ignores stale files plus `.crdownload`, `.part`, `.partial`, and `.download` files.

```powershell
cmg browser control events waitForDownload [options]
```

## Options

- `--directory <path>`: Directory to watch. Default is the current directory.
- `--pattern <glob>`: File glob to match. Default is `*`.
- `--timeout <milliseconds>`: Timeout in milliseconds. Default is `5000`.

## Stdout

```text
DOWNLOAD 001 C:\path\file.csv
```

## Stderr

Writes file or timeout errors.

## Exit Codes

- `0`: A new or changed matching download reached a stable completed state.
- `1`: The action failed.
