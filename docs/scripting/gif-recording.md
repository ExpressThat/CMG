# GIF Recording

Scripts can record an animated GIF with:

```powershell
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif
```

## What Gets Captured

- CMG captures the visible page viewport.
- A frame is captured after every action.
- Click, type, clear, hover, scroll, screenshot, html, assert, and wait actions move the virtual pointer to the target selector when possible.
- Click actions show a pointer movement and click pulse.
- Type actions move to the input, click/focus it, and capture frames as characters appear.
- Drag-and-drop actions move from the source selector to the target selector using real browser mouse drag events, so pages can show their own native drag preview when one is available.
- Delay actions capture a hold frame.

## Virtual Pointer

GIF recording uses one pointer:

- A lightweight DOM pointer is injected into the page while the script runs, so the live browser visibly moves.
- The pointer is styled as a standard arrow pointer, with a transparent popover surface and no default popover box.
- The pointer uses the browser top layer through a manual popover when available, so dialogs and high `z-index` page elements do not cover it.
- CMG re-promotes the pointer in the top layer before frame capture, so newly opened dialogs do not cover an already existing pointer.
- The GIF captures this same injected pointer from the browser screenshot. CMG does not draw a second overlay pointer onto GIF frames.

CMG removes the DOM cursor when recording ends.

## Drag Ghost

During recorded `dragAndDrop`, CMG sends real Chrome mouse down, move, and release events while recording frames. It also dispatches the page drag lifecycle while the pointer moves: `dragstart`, repeated `drag` and `dragover` events, then `dragend`.

If the page calls `DataTransfer.setDragImage()` during `dragstart`, CMG treats that as a site-owned custom drag image and does not add its own drag preview. Pages that render their own DOM-based drag ghost during drag events also continue to control their own visual behavior.

When the page does not call `DataTransfer.setDragImage()`, a manual browser drag would normally show Chrome's generated default drag preview. DevTools-driven drags do not reliably expose that native preview to the live browser view or page screenshots, so CMG adds a default-preview bridge by cloning the source element into the browser top layer while the drag is active. This is only used for the default-preview case; custom page drag images take precedence.

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
- GIF recording is supported only on `browser control script`.
