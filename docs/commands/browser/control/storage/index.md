# `browser control storage`

Storage and persisted browser state command group.

```powershell
cmg browser control storage [command] [options]
```

## Subcommands

- [`local`](local.md): Read or mutate `localStorage`.
- [`session`](session.md): Read or mutate `sessionStorage`.
- [`cookie`](cookie.md): Read or mutate `document.cookie`.
- [`state`](state.md): Save or load localStorage, sessionStorage, and cookies.
- [`storageState`](storageState.md): Save or load localStorage, sessionStorage, and cookies.

## Behavior

- Requires a browser started with [`browser launch`](../../launch.md).
- Runs the same underlying scripting actions as `browser control script`.
- Writes `PASS` and storage output lines to stdout.
- Writes browser, argument, parse, file, or action errors to stderr.
- Exits `0` on success and `1` on failure.

## Examples

```powershell
cmg browser control storage local set token abc
cmg browser control storage local get token
cmg browser control storage session clear
cmg browser control storage cookie get
cmg browser control storage state save --path artifacts\auth.json
cmg browser control storage storageState load --path artifacts\auth.json
```
