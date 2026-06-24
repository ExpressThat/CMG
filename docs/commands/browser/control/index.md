# `browser control`

Browser interaction and page control command group.

```powershell
cmg browser control [command] [options]
```

## Behavior

- Contains commands that interact with the active page through Chrome DevTools Protocol.
- Run [`browser launch`](../launch.md) before using control commands.
- Running `browser control` without a subcommand exits with `1`.

## Subcommands

- [`getElement`](getElement.md): Return HTML or a screenshot for a selected element.
- [`script`](script.md): Run a `.cmgscript` browser automation script.

## Examples

```powershell
cmg browser control --help
cmg browser control getElement "h1" --html
cmg browser control script --file flow.cmgscript
```
