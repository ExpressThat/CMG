# `browser control tabs listTabs`

Runs the scripting `listTabs` action once from the command line.

```powershell
cmg browser control tabs listTabs
```

## Stdout

```text
PASS 001 listTabs
TAB 0 active=true url="about:blank"
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Tabs were listed.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control tabs listTabs
```
