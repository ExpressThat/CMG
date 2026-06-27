# `browser control input scroll`

Window, element, and wheel scrolling commands.

```powershell
cmg browser control input scroll [command] [options]
```

## Subcommands

- [`to`](to.md): Scroll to an absolute position or alias.
- [`scrollTo`](scrollTo.md): Scroll to an absolute position or alias.
- [`by`](by.md): Scroll by a delta.
- [`scrollBy`](scrollBy.md): Scroll by a delta.
- [`wheel`](wheel.md): Dispatch a wheel event and scroll.

## Examples

```powershell
cmg browser control input scroll to bottom
cmg browser control input scroll scrollBy 0 250 --selector "#pane"
cmg browser control input scroll by --x 0 --y -80 --selector "text=Panel"
cmg browser control input scroll wheel "#pane" --delta-y 120
```
