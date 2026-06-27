# `browser control events console listConsole`

Lists console messages captured in the page-side `window.__cmgConsole` buffer.

CMG arms console diagnostics automatically when `browser launch`, `browser app launch`, or `browser app attach` succeeds. This command reads the current page buffer; it does not clear entries. Capture is forward-only from launch/attach/arming time.

```powershell
cmg browser control events console listConsole [text] [options]
```

## Arguments

- `[text]`: Optional console message text to include.

## Options

- `--level <log|info|warn|error>`: Optional console level filter.
- `--match <contains|exact|regex>`: Text match mode. Defaults to `contains`.
- `--ignore-case`: Match text without case sensitivity.

## Stdout

```text
PASS 001 line=1 action=listConsole
CONSOLE_LIST 001 count=<count>
CONSOLE_ENTRY 001 index=<index> level=<level> text=<message>
```

Empty lists succeed:

```text
PASS 001 line=1 action=listConsole
CONSOLE_LIST 001 count=0
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Console entries were listed, including when the list is empty.
- `1`: Browser is not running, an option is invalid, or the action failed.

## Examples

```powershell
cmg browser control events console listConsole
cmg browser control events console listConsole --level error
cmg browser control events console listConsole "^failed" --match regex --ignore-case --level error
```

