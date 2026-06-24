# `browser`

Browser lifecycle and capture command group.

```powershell
cmg browser [command] [options]
```

## Subcommands

- [`launch`](launch.md): Launch a CMG-controlled browser instance with remote debugging enabled.
- [`close`](close.md): Close the CMG-controlled browser instance.
- [`control`](control/index.md): Browser interaction and page control commands.

## Examples

```powershell
cmg browser launch
cmg --chrome browser launch
cmg --edge browser launch
cmg --firefox browser launch
cmg browser close
cmg browser control --help
```
