# `.cmgscript` Actions

All actions fail fast. If an action fails, later actions are not executed.

## `navigate`

```text
navigate "<url-or-path>"
```

Navigates the primary page target to a URL, data URL, or local file path.

Example:

```text
navigate "C:\Projects\CMG\index.html"
```

## `waitForElement`

```text
waitForElement "<selector>" timeout=5000
```

Waits until a selector exists in any available page target.

Options:

- `timeout`: Optional timeout in milliseconds. Default is `5000`.

Example:

```text
waitForElement "#openProfileDialog" timeout=5000
```

## `click`

```text
click "<selector>"
```

Scrolls the element into view and dispatches a Chrome DevTools mouse press and release at the element center.

Example:

```text
click "#openProfileDialog"
```

## `type`

```text
type "<selector>" "text"
```

Focuses the element, appends text to its current value, and dispatches `input` and `change` events.

Example:

```text
type "#profileName" "CMG Test Profile"
```

## `clear`

```text
clear "<selector>"
```

Focuses the element, clears its value, and dispatches `input` and `change` events.

Example:

```text
clear "#profileName"
```

## `press`

```text
press "Enter"
```

Dispatches a key down and key up event through Chrome DevTools Protocol.

Example:

```text
press "Escape"
```

## `hover`

```text
hover "<selector>"
```

Dispatches mouseover and mousemove events at the element center.

Example:

```text
hover "#openProfileDialog"
```

## `scrollIntoView`

```text
scrollIntoView "<selector>"
```

Scrolls an element into the center of the viewport.

Example:

```text
scrollIntoView "#dragdrop"
```

## `select`

```text
select "<selector>" "value"
```

Sets a select-like element value and dispatches `input` and `change` events.

Example:

```text
select "#environment" "prod"
```

## `delay`

```text
delay 1000
```

Pauses execution for the specified number of milliseconds.

## `html`

```text
html "<selector>"
```

Prints the selected element's `outerHTML` to stdout as an `HTML` result line.

Example:

```text
html "#openProfileDialog"
```

## `screenshot`

```text
screenshot "<selector>" output="element.png"
screenshot "<selector>"
```

Captures a PNG screenshot of an element.

Options:

- `output`: Optional file path. Without it, stdout receives a `data:image/png;base64,...` result.

## `screenshotPage`

```text
screenshotPage output="page.png"
screenshotPage
```

Captures a PNG screenshot of the primary page target.

Options:

- `output`: Optional file path. Without it, stdout receives a `data:image/png;base64,...` result.

## `assertText`

```text
assertText "<selector>" "expected text"
```

Reads an element's visible text and fails unless it contains the expected text.

Example:

```text
assertText "#lastDialogAction" "None"
```

## `evaluate`

```text
evaluate "document.title"
```

Evaluates JavaScript in the primary page target and prints the returned value as an `EVALUATE` result line.

## `setViewport`

```text
setViewport width=1280 height=720
```

Sets viewport metrics for the primary page target.

Required options:

- `width`
- `height`

## `dragAndDrop`

```text
dragAndDrop "<sourceSelector>" "<targetSelector>"
```

Dispatches drag-and-drop DOM events from the source element to the target element.

Example:

```text
dragAndDrop "[data-command='browser launch']" "#dropQueue"
```

## `listTabs`

```text
listTabs
```

Prints available Chrome page targets as `TAB` result lines.

## `activateTab`

```text
activateTab index=0
```

Activates a tab by index from `listTabs`.

Required options:

- `index`

## `closeTab`

```text
closeTab index=1
```

Closes a tab by index from `listTabs`.

Required options:

- `index`

## `set`

```text
set name "value"
```

Stores a variable for later `${name}` expansion.

Example:

```text
set target "#openProfileDialog"
click "${target}"
```
