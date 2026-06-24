# `browser`

Browser lifecycle and capture command group.

```powershell
cmg browser [command] [options]
```

## Subcommands

- [`launch`](launch.md): Launch a CMG-controlled Chrome instance with remote debugging enabled.
- [`close`](close.md): Close the CMG-controlled Chrome instance.
- [`control`](control/index.md): Browser interaction and page control commands.

## Examples

```powershell
cmg browser launch
cmg browser close
cmg browser control --help
```
