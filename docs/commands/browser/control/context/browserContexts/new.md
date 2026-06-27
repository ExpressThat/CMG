# `browser control context browserContexts new`

Creates and activates a fresh browser context.

```powershell
cmg browser control context browserContexts new [--url <target>]
```

## Options

- `--url <target>`: Initial URL. Default is `about:blank`.

## Stdout

```text
PASS 001 newContext url=about:blank
CONTEXT_CREATED 001 id=<context-id> target=<target-id> url="about:blank"
```

## Exit Codes

- `0`: Context was created and activated.
- `1`: Browser is not running or the action failed.
