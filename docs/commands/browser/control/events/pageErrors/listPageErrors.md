# `browser control events pageErrors listPageErrors`

Lists page errors captured in the page-side `window.__cmgPageErrors` buffer, including `error` and `unhandledrejection` events.

CMG arms page-error diagnostics automatically when `browser launch`, `browser app launch`, or `browser app attach` succeeds. This command reads the current page buffer; it does not clear entries. Capture is forward-only from launch/attach/arming time.

```powershell
cmg browser control events pageErrors listPageErrors [text] [options]
```

## Arguments

- `[text]`: Optional page-error text to include.

## Options

- `--match <contains|exact|regex>`: Text match mode. Defaults to `contains`.
- `--ignore-case`: Match text without case sensitivity.

## Stdout

```text
PASS 001 line=1 action=listPageErrors
PAGE_ERROR_LIST 001 count=<count>
PAGE_ERROR_ENTRY 001 index=<index> type=<type> source=<source> line=<pageLine> column=<pageColumn> text=<message>
```

Empty lists succeed:

```text
PASS 001 line=1 action=listPageErrors
PAGE_ERROR_LIST 001 count=0
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Page errors were listed, including when the list is empty.
- `1`: Browser is not running, an option is invalid, or the action failed.

## Examples

```powershell
cmg browser control events pageErrors listPageErrors
cmg browser control events pageErrors listPageErrors "Cannot read"
cmg browser control events pageErrors listPageErrors "^TypeError" --match regex --ignore-case
```

