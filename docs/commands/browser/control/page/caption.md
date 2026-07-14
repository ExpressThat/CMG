# `browser control page caption`

Runs the scripting `caption` action once from the command line.

```powershell
cmg browser control page caption "<message>" [options]
```

This is an exact-name alias for [`showMessageBar`](showMessageBar.md). It is intended for visual GIF narration and updates the same CMG-owned caption bar used by script GIF recording.

## Arguments

- `<message>`: Message text to show in a fixed bar at the top of the page.

## Options

Supports the same `--target`, `--position`, `--style`, `--severity`, and `--size` options as [`showMessageBar`](showMessageBar.md#options). In particular, `--target <selector-or-rich-locator> --position auto` places the caption opposite the target without moving a virtual pointer.

## Behavior

Injects or updates a CMG-owned DOM caption bar near the top center of the current page. The bar dynamically sizes to the message, wraps longer text across multiple lines, uses the browser top layer through a manual popover when available, is visible above browser dialogs in screenshots and GIF recordings, and does not intercept pointer input.

Running the command again updates the existing bar text.

## Stdout

```text
PASS 001 caption "Manual sign-in required"
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Caption bar was injected or updated.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control page caption "Opening the profile dialog"
cmg browser control page caption "Approve the release" --target "#approve" --position auto
```

Multi-line captions can use escaped newlines in scripts:

```text
caption "Opening profile dialog\nWaiting for dialog content"
```
