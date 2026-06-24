# `browser control set`

Runs the scripting `set` action once from the command line.

```powershell
cmg browser control set "<name>" "<value>"
```

## Arguments

- `<name>`: Variable name.
- `<value>`: Variable value.

## Behavior

This command exists for parity with `.cmgscript` actions. Because each command-line action is an isolated one-action script invocation, the variable is discarded when the command exits and is not available to later commands.

Use [`script`](script.md) when you need variables across multiple actions.

## Stdout

```text
PASS 001 set name value
```

## Stderr

Writes browser or validation errors.

## Exit Codes

- `0`: The one-action invocation succeeded.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control set target "#openProfileDialog"
```
