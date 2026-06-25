# `browser control getElement`

Returns HTML or a screenshot for a selected element on the active page.

```powershell
cmg browser control getElement <selector> (--html | --screenshot) [--output <path>]
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator for the target element.

## Options

- `--html`: Print the selected element's `outerHTML`.
- `--screenshot`: Capture a PNG screenshot of the selected element.
- `--sscreenshot`: Alias for `--screenshot`.
- `--output <path>`: Write screenshot PNG bytes to a file instead of printing base64 to stdout.

## Behavior

- Connects to the selected CMG browser automation endpoint.
- Searches available page targets until the selector or locator is found.
- Supports the same rich locators as scripts, including `text=`, `role=`, `label=`, `testid=`, `placeholder=`, `alt=`, `title=`, and `xpath=`.
- `--html` reads the selected element without scrolling.
- `--screenshot` scrolls the selected element into view before capture.
- Exactly one output mode must be provided: `--html` or `--screenshot`.
- Quote selectors such as `"#openProfileDialog"` in shells where `#` can start a comment.

## Stdout

With `--html`, stdout is the selected element HTML:

```html
<button id="target">Hello</button>
```

With `--screenshot`, stdout is a PNG data URL:

```text
data:image/png;base64,<base64-png-data>
```

With `--screenshot --output <path>`, stdout is the output path:

```text
C:\path\to\element.png
```

## Stderr

Validation and runtime errors are written to stderr, including:

- Missing browser launch state.
- Missing selector match.
- Both `--html` and `--screenshot` provided.
- Neither `--html` nor `--screenshot` provided.

## Exit Codes

- `0`: Element HTML or screenshot was returned successfully.
- `1`: Validation failed, the selected browser was unavailable, or the element could not be found/captured.

## Examples

```powershell
cmg browser launch https://example.com
cmg --edge browser launch https://example.com
cmg --firefox browser launch https://example.com
cmg browser control getElement "h1" --html
cmg browser control getElement "text=Save changes" --html
cmg --edge browser control getElement "h1" --html
cmg browser control getElement "#hero" --screenshot
cmg browser control getElement ".card" --screenshot --output card.png
```
