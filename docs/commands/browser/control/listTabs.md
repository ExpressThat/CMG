# `browser control listTabs`

Runs the scripting `listTabs` action once from the command line.

```powershell
cmg browser control listTabs
```

## Stdout

Prints a `PASS` line and one `TAB` line per page target:

```text
PASS 001 listTabs
TAB 0 id=... title="CMG Browser Control Test Page" url="file:///C:/Projects/CMG/index.html"
```

## Stderr

Writes browser errors.

## Exit Codes

- `0`: Tabs were listed.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control listTabs
```
