# `browser control context browserContexts newContext`

Runs the scripting `newContext` action once from the command line.

```powershell
cmg browser control context browserContexts newContext [--url <target>]
```

## Options

- `--url <target>`: Initial URL. Default is `about:blank`.

## Stdout

```text
PASS 001 newContext url=about:blank
CONTEXT_CREATED 001 id=<context-id> target=<target-id> url="about:blank"
```

## Stderr

Writes browser, navigation, parse, or action errors.

## Exit Codes

- `0`: Context was created and activated.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control context browserContexts newContext --url "about:blank"
```
