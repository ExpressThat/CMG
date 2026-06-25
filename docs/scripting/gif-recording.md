# GIF Recording

Scripts can record an animated GIF with:

```powershell
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif
cmg run flow.cmgscript --gif demo-output\runner-gifs
```

## What Gets Captured

- CMG captures the visible page viewport.
- A frame is captured after visual actions. The `set` variable action is logged but does not add a standalone frame because it has no page-visible effect.
- Click, type, clear, hover, select, and drag actions move the virtual pointer to the target selector when possible. User-like movement actions do not scroll automatically; add `scrollIntoView` steps when the pointer should move to content outside the current viewport.
- Low-level `mouseMove`, `mouseDown`, and `mouseUp` actions also move the virtual pointer in GIF mode and dispatch page-facing pointer/mouse events.
- Low-level keyboard actions do not move the pointer, but their output and failures are recorded. Use `step` or `caption` when a GIF should explain held modifier keys or inserted text.
- Every GIF pointer movement dispatches browser movement and hover events while it moves. This includes automatic movement before `click`, `type`, `clear`, `hover`, `select`, and `dragAndDrop`, not only explicit drag movement.
- Click actions show a pointer movement and click pulse.
- Type actions move to the input, click/focus it, and capture frames as characters appear.
- Drag-and-drop actions move from the source selector to the target selector while keeping the page drag lifecycle active, so pages can show their own native drag preview when one is available.
- During a block drag, CMG dispatches DOM `pointerdown`/`mousedown` at drag start, held `pointermove`/`mousemove` while moving or delaying, and `pointerup`/`mouseup` at drop. The Chrome/Edge pointer also moves through CDP. This lets page drag state and edge-autoscroll logic react without forcing Chrome into native drag mode.
- Screenshot actions still scroll the selected element into view before capture.
- Pointer actions such as `click`, `dblclick`, `rightClick`, `type`, `fill`, `clear`, `hover`, `select`, `selectOption`, `check`, `uncheck`, `focus`, `blur`, `selectText`, and `dragAndDrop` move the virtual pointer before acting so recorded GIFs show the same target the browser receives.
- File, fixture, init-script, runtime tag injection, explicit event dispatch, PDF, API, explicit wait, navigation wait, dialog, network environment, network mock, worker, coverage, page-error, clock, browser context, accessibility, element-state assertion, and storage actions such as `localStorage`, `sessionStorage`, and `cookie` do not move the virtual pointer. Wrap them in `step`, `caption`, or `gif` blocks when their result should be visible to a GIF viewer.
- Delay actions capture a hold frame.

## Virtual Pointer

GIF recording uses one pointer:

- A lightweight DOM pointer is injected into the page while the script runs, so the live browser visibly moves.
- The pointer is styled as a standard arrow pointer, with a transparent popover surface and no default popover box.
- The pointer uses the browser top layer through a manual popover when available, so dialogs and high `z-index` page elements do not cover it.
- CMG re-promotes the pointer in the top layer before frame capture, so newly opened dialogs do not cover an already existing pointer.
- The GIF captures this same injected pointer from the browser screenshot. CMG does not draw a second overlay pointer onto GIF frames.

CMG removes the DOM cursor when recording ends.

## `moveMouse`

`moveMouse` is a script-only action for GIF runs. It has no one-off CLI equivalent.

```text
moveMouse "center"
moveMouse x=100 y=200
moveMouse selector=".content-area" edge=bottom inset=24
```

Coordinates are viewport-relative CSS pixels. Aliases are inset from the viewport edge where needed: `center`, `top`, `bottom`, `left`, `right`, `topLeft`, `topRight`, `bottomLeft`, and `bottomRight`.

Use selector/edge targeting when the app scrolls a specific container rather than the browser window:

```text
moveMouse selector=".content-area" edge=bottom inset=24
```

Use `moveMouse selector=".content-area" edge=bottom` with `delay` inside a `dragAndDrop` block when a page auto-scrolls while a dragged item is held near the lower edge of a scrollable content area. CMG keeps the drag state active during the delay and repeatedly emits held pointer/mouse movement plus dragover events:

```text
dragAndDrop ".card" {
  moveMouse selector=".content-area" edge=bottom inset=24
  delay 1500
  moveMouse selector=".content-area" edge=bottom inset=24
  delay 1500
  drop "#target"
}
```

## Drag Ghost

During recorded `dragAndDrop`, CMG moves the virtual pointer while recording frames and dispatches the page drag lifecycle: `dragstart`, repeated `drag` and `dragover` events, then `dragend`.

CMG creates the `DataTransfer` object needed for synthetic drag events, then lets the page's own drag handlers set `effectAllowed`, `dropEffect`, and drag payloads. CMG does not force those values; it preserves page-set values through the recorded drag so `dragover` handlers can inspect them.

Recorded GIF drags use one synthetic drag lifecycle and dispatch exactly one `drop` event. For block `dragAndDrop { ... }` scripts, the `drop "<selector>"` child dispatches that single `drop`. CMG does not also run a native or fallback drop after the recorded drop completes.

If the page calls `DataTransfer.setDragImage()` during `dragstart`, CMG treats that as a site-owned custom drag image and does not add its own drag preview. Pages that render their own DOM-based drag ghost during drag events also continue to control their own visual behavior.

When the page does not call `DataTransfer.setDragImage()`, a manual browser drag would normally show the browser's generated default drag preview. Automation-driven drags do not reliably expose that native preview to the live browser view or page screenshots, so CMG adds a default-preview bridge by cloning the source element into the browser top layer while the drag is active. This is only used for the default-preview case; custom page drag images take precedence.

After the visual mouse drag finishes, CMG still applies the normal scripted drop behavior used by the non-GIF `dragAndDrop` action. This keeps command execution reliable for pages that accept scripted drag events but do not complete a drop from DevTools mouse events alone.

## Output

On success, stdout includes:

```text
GIF C:\Projects\CMG\demo-output\flow.gif
```

On failure, CMG still writes a partial GIF when at least one frame was captured, then exits with code `1`.

## Timing

V1 uses balanced default timing:

- about 10 frames per second
- smooth pointer movement between targets
- short holds after actions
- a visible click pulse for click and drag/drop actions

Timing is not configurable in v1.

## Notes

- GIF files can become large for long scripts.
- Recording can make scripts run slower because screenshots are captured after each action.
- GIF recording is supported by `browser control script --gif`, `cmg run --gif`, and `gif "name" { ... }` blocks inside `cmg run` scripts.
- `moveMouse` requires `--gif`; scripts without recording do not create or move a virtual mouse.
