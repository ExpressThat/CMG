# GIF Recording

Scripts can record an animated GIF with:

```powershell
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif --gif-quality highest
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif --pointer-duration 600 --pointer-easing ease-in-out
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif --pointer-theme ring --pointer-color "#dc2626" --pointer-size 44
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif --show-pointer false
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif --caption-style qa --caption-position bottom
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif --gif-timeline demo-output\timelines
cmg run flow.cmgscript --gif demo-output\runner-gifs
cmg run flow.cmgscript --gif demo-output\runner-gifs --gif-timeline demo-output\timelines
cmg run flow.cmgscript --gif demo-output\runner-gifs --gif-quality highest
```

## What Gets Captured

- CMG captures the visible page viewport.
- A frame is captured after visual actions. The `set` variable action is logged but does not add a standalone frame because it has no page-visible effect.
- Navigation waits such as `waitForNetworkIdle` are logged and traced but do not move the virtual pointer or add pointer motion frames.
- Environment actions such as `emulateMedia` are also non-visual by themselves; later page changes and visual actions are recorded normally with the same virtual pointer.
- Explicit `screenshot` and `screenshotPage` actions can write PNG or JPEG artifacts with `type=` and `quality=`. They can apply temporary artifact-only CSS with `style=` or `stylePath=`, artifact-only masks with `mask=` and `maskColor=`, disabled artifact animations with `animations=disabled`, and hidden artifact carets with `caret=hide`. `screenshotPage` can also write clipped artifacts with `clipX=`, `clipY=`, `clipWidth=`, and `clipHeight=`. GIF recorder frames always use CMG's internal PNG capture path so virtual pointer compositing and frame encoding stay consistent.
- Visual assertions can use `fullPage=true` and `mask=` for comparison artifacts. Masking changes only the captured assertion image; selector assertions still move the virtual pointer before the comparison frame, and page-level assertions still do not move it.
- Screenshot artifact masks, animation disabling, and caret hiding do not change GIF frames. If the viewer should see a redaction, frozen animation, or highlight in the GIF itself, change the page with `evaluate`, `highlight`, `caption`, or normal page actions before recording that frame.
- Click, type, clear, hover, select, wheel, and drag actions move the virtual pointer to the target selector when possible. User-like movement actions do not scroll automatically; add `scrollIntoView`, `scrollTo`, `scrollBy`, or `wheel` steps when the pointer should move to content outside the current viewport.
- Pointer actions that accept element offsets, such as `click "<selector>" x=8 y=12` and `hover "<selector>" x=8 y=12`, move the virtual pointer to the same element-relative point that receives the browser event.
- `type`, `pressSequentially`, and `fill` use progressive typing during GIF capture so typed characters can produce frames. Add `delay=<milliseconds>` to control the per-character pace; omit it for the default visual typing delay.
- Rich locators such as `getByLabel=`, `getByRole=`, `nth=`, `has=`, `hasNot=`, `hasText=`, `hasNotText=`, `visible=`, `or=`, `and=`, `strict=`, `inside=`, `closest=`, `parent=`, `next=`, `previous=`, `shadow=`, and `shadowText=` resolve before pointer movement. Their marker keeps a live resolver, so dynamically remounted elements are reacquired during the same recorded action instead of retaining a disconnected DOM node.
- Low-level `mouseMove`, `mouseDown`, and `mouseUp` actions also move the virtual pointer in GIF mode and dispatch page-facing pointer/mouse events.
- Low-level keyboard actions such as `press`, `keyboardShortcut`, `shortcut`, `hotkey`, `keyDown`, `keyUp`, and `insertText` do not move the pointer, but their output and failures are recorded. Use `step` or `caption` when a GIF should explain held modifier keys, `press delay=`, or inserted text.
- Every GIF pointer movement dispatches browser movement and hover events while it moves. This includes automatic movement before `click`, `tap`, `touchTap`, `type`, `clear`, `hover`, `select`, and `dragAndDrop`, not only explicit drag movement.
- Click actions dispatch one browser activation and then show a visual-only click pulse. The pulse and caption path never call the target, including label/control targets. `tap` and `touchTap` use CMG's touch pointer ring so touch-style flows read differently from mouse clicks.
- Type actions move to the input, click/focus it, and capture frames as characters appear.
- `wheel` moves the pointer when given a selector, alias, or coordinates, dispatches a page `WheelEvent`, and captures the scrolled viewport. `scrollTo` and `scrollBy` scroll the window or selected element and capture the changed viewport without moving the pointer.
- Drag-and-drop actions move from the source selector to the target selector while keeping the page drag lifecycle active, so pages can show their own native drag preview when one is available.
- Simple `dragAndDrop` and `dragTo` source/target offsets move the virtual pointer and drag ghost through the same element-relative points that receive page drag events.
- During a block drag, CMG dispatches DOM `pointerdown`/`mousedown` at drag start, held `pointermove`/`mousemove` while moving or delaying, and `pointerup`/`mouseup` at drop. The Chrome/Edge pointer also moves through CDP. This lets page drag state and edge-autoscroll logic react without forcing Chrome into native drag mode.
- Screenshot actions still scroll the selected element into view before capture.
- Pointer actions such as `click`, `dblclick`, `doubleClick`, `rightClick`, `contextClick`, `tap`, `touchTap`, `type`, `pressSequentially`, `fill`, `clear`, `hover`, `highlight`, `select`, `selectOption`, `check`, `uncheck`, `focus`, `blur`, `selectText`, `wheel`, `uploadFiles`, `setInputFiles`, `selectFile`, `expectScreenshot`, `toHaveScreenshot`, `dragAndDrop`, and `dragTo` move the virtual pointer before acting so recorded GIFs show the same target the browser receives.
- File, fixture, init-script, runtime tag injection, explicit event dispatch, keyboard shortcut, PDF, API, explicit wait, navigation wait, clipboard, dialog, browser environment, named-device emulation, network environment, network mock, network wait, WebSocket, worker, coverage, tracing, console, page-error, clock, browser context, accessibility, element getters such as `count`, `boundingBox`, and `allTextContents`, evaluated assertion, text assertion, explicit `fail`, element-state assertion, and storage actions such as `localStorage`, `sessionStorage`, and `cookie` with cookie attributes do not move the virtual pointer. Wrap them in `step`, `caption`, or `gif` blocks when their result should be visible to a GIF viewer.
- Structural actions such as `import`, `macro`, `call`, `return`, `within`, `frame`, `frameLocator`, `if`, `elseif`, `else`, `switch`, `case`, `default`, `for`, `repeat`, `while`, `until`, `doWhile`, `doUntil`, `retry`, `toPass`, `foreach`, `foreachSelector`, `break`, `continue`, `try`, `catch`, and `finally` do not move the virtual pointer by themselves. Pointer-aware actions inside those blocks still use the same virtual pointer movement, pointer events, hover behavior, drag ghosts, and captions as top-level actions.
- Delay actions capture a hold frame.

## Virtual Pointer

GIF recording uses one pointer:

- A lightweight DOM pointer is injected into the page while the script runs, so the live browser visibly moves.
- The pointer is styled as a standard arrow pointer, with a transparent popover surface and no default popover box.
- Pointer visuals can be changed with `pointerTheme=`, `pointerColor=`, `pointerSize=`, `pointerShadow=`, and `showPointer=` on `recording`, `gif`, `recordVideo`, `screencast`, and pointer-aware child actions. Command-level `--pointer-theme`, `--pointer-color`, `--pointer-size`, `--pointer-shadow`, and `--show-pointer` set whole-run defaults.
- The pointer uses the browser top layer through a manual popover when available, so dialogs and high `z-index` page elements do not cover it.
- CMG re-promotes the pointer in the top layer before frame capture, so newly opened dialogs do not cover an already existing pointer.
- The GIF captures this same injected pointer from the browser screenshot. CMG does not draw a second overlay pointer onto GIF frames.

CMG removes the DOM cursor when recording ends.

## Pointer Choreography

Use pointer choreography options when a recording should be slower, faster, or more deliberately staged than the defaults:

```text
gif "checkout evidence" pointerDuration=650 pointerEasing=ease-in-out {
  click "#plan-pro"
  click "#continue" pointerDuration=250 pointerTheme=hand pointerColor="#16a34a"
}
```

Supported scoped recording options on `gif`, `recordVideo`, and `screencast` blocks:

- `pointerDuration=<milliseconds>`: Movement duration between pointer targets. Must be zero or greater.
- `pointerSpeed=<slow|normal|fast|instant|multiplier>`: Preset or multiplier such as `1.5x`.
- `pointerEasing=<linear|ease-in|ease-out|ease-in-out|spring>`: Movement curve.
- `pointerPath=<direct|arc|manhattan|avoid-target|avoid-center>`: Pointer route between targets. Defaults to `direct`.
- `dragPath=<direct|arc|manhattan|avoid-target|avoid-center>`: Drag route while the pointer is held. Defaults to `pointerPath` when omitted.
- `pointerTheme=<arrow|hand|dot|ring|branded|touch>`: Virtual pointer visual theme. `tap` and `touchTap` use `touch` automatically when the inherited theme is the default arrow.
- `pointerColor=<css-color>`: Pointer color. Use a single CSS color value, not a declaration.
- `pointerSize=<auto|8..96>`: Pointer size in CSS pixels. `auto` uses the theme's designed size.
- `pointerShadow=<none|light|medium|strong>`: Pointer drop shadow strength. Defaults to `medium`.
- `showPointer=<true|false|auto>`: Pointer visibility for captured frames. Defaults to `auto`, which currently shows the pointer for pointer-aware evidence frames. Use `false` for clean page-state frames; child actions can override with `showPointer=true`.
- `captionStyle=<subtle|teaching|qa|bug-report|compact>`: Caption style default for child captions and step title bars.
- `captionPosition=<top|bottom|left|right|auto>`: Caption placement default.
- `captionSeverity=<info|success|warning|error>`: Caption color intent default.
- `autoCaptions=<true|false>`: Narrate supported visual actions automatically. Defaults to `false`; children can override it.
- `captionTemplate=<template>`: Automatic-caption template with `{action}`, `{selector}`, `{target}`, `{line}`, and `{arguments}`.
- `intro=<text>` / `outro=<text>`: Full-viewport title cards captured before the first child and at finalization.
- `introDuration=<milliseconds>` / `outroDuration=<milliseconds>`: Title-card holds. Defaults to `1200` and must be greater than zero.
- `clickPulse=<ring|ripple|dot|crosshair|none>`: Click/tap/drop pulse style. Defaults to `ring` because clicks should be visible evidence by default.
- `holdAfterAction=<milliseconds>`: Post-action hold duration. Defaults to `350`; use `0` to suppress the hold for a block or action.
- `preClickHold=<milliseconds>`: Hold after pointer movement and before click/tap dispatch. Defaults to `0`.
- `postClickHold=<milliseconds>`: Hold after click/tap pulse frames. Defaults to `350`. Action-level `holdAfterAction=` remains a general fallback when `postClickHold=` is not set.
- `holdAfterMove=<milliseconds>`: Default hold after recorded `moveMouse` actions. Child `moveMouse` actions can override it locally.
- `holdAfterNavigation=<milliseconds>`: Hold after navigation actions and navigation waits. Defaults to `350`.
- `holdAfterAssertion=<milliseconds>`: Hold after assertion actions. Defaults to `350`.
- `holdOnFailure=<milliseconds>`: Extra final-state hold captured only when the recording fails. Defaults to `1200`; use `0` to suppress the failure hold.
- `fps=<1..100>`: Frame rate for this recording block. Defaults to `10`.
- `frameDelay=<milliseconds>`: Frame delay for this recording block. Must be `10..10000`; overrides `fps=`.
- `timeline=<true|false|file|directory>`: Optional timeline JSON sidecar for this recording block. `true` writes `<gif-name>.timeline.json` next to the GIF; a directory writes the same filename inside that directory; a `.json` path writes that exact file.

The same options can be set on pointer-aware child actions. A block's options are defaults for everything inside that block, and child actions can override them for only that action.

Use `recording { ... }` or `withRecording { ... }` to set scoped recording defaults without starting a recording by itself. The scope inherits through macros, loops, `try` / `catch` / `finally`, `within`, frame blocks, nested `gif` / `recordVideo` / `screencast` blocks, and command-level `--gif` runs. It still follows CMG's virtual pointer rule: without `--gif` or a nested recording block, no virtual pointer is injected and recording-only actions skip.

```text
recording pointerDuration=300 pointerEasing=ease-in-out clickPulse=dot holdAfterAction=500 {
  gif "checkout evidence" {
    click "#plan-pro"
    click "#continue" pointerDuration=700 clickPulse=ripple
  }
}
```

Supported `recording` / `withRecording` defaults:

- `quality=<highest|high|medium|low>`: Default for nested recording blocks that create their own artifact.
- `pointerDuration=<milliseconds>`: Movement duration between pointer targets.
- `pointerSpeed=<slow|normal|fast|instant|multiplier>`: Preset or multiplier such as `1.5x`.
- `pointerEasing=<linear|ease-in|ease-out|ease-in-out|spring>`: Movement curve.
- `pointerPath=<direct|arc|manhattan|avoid-target|avoid-center>`: Pointer route between targets.
- `dragPath=<direct|arc|manhattan|avoid-target|avoid-center>`: Drag route while the pointer is held.
- `pointerTheme=<arrow|hand|dot|ring|branded|touch>`: Default pointer visual theme.
- `pointerColor=<css-color>`: Default pointer color.
- `pointerSize=<auto|8..96>`: Default pointer size in CSS pixels.
- `pointerShadow=<none|light|medium|strong>`: Default pointer shadow.
- `showPointer=<true|false|auto>`: Default pointer visibility for captured frames.
- `captionStyle=<subtle|teaching|qa|bug-report|compact>`: Default caption style.
- `captionPosition=<top|bottom|left|right|auto>`: Default caption position.
- `captionSeverity=<info|success|warning|error>`: Default caption severity.
- `autoCaptions=<true|false>`: Automatic narration default for supported child actions.
- `captionTemplate=<template>`: Automatic-caption template inherited by child actions.
- `intro=<text>` / `outro=<text>`: Optional opening and final title cards.
- `introDuration=<milliseconds>` / `outroDuration=<milliseconds>`: Optional title-card durations. Defaults to `1200`.
- `pressedPointer=<true|false>`: Whether the pointer visually compresses while a recorded drag is active. Defaults to `true`.
- `dragTrail=<true|false>`: Draw a trailing line behind held-pointer drag movement. Defaults to `false`.
- `dragBreadcrumbs=<true|false>`: Add dot breadcrumbs along held-pointer drag movement. Defaults to `false`.
- `clickPulse=<ring|ripple|dot|crosshair|none>` or `pulse=<...>`: Click/tap/drop pulse style.
- `holdAfterAction=<milliseconds>`: Post-action hold duration.
- `preClickHold=<milliseconds>`: Hold before click/tap dispatch after pointer movement.
- `postClickHold=<milliseconds>`: Hold after click/tap pulse frames.
- `holdAfterMove=<milliseconds>`: Hold after recorded `moveMouse` movement.
- `holdAfterNavigation=<milliseconds>`: Hold after navigation actions and waits.
- `holdAfterAssertion=<milliseconds>`: Hold after assertion actions.
- `holdOnFailure=<milliseconds>`: Final failure-state hold for nested recording blocks.
- `fps=<1..100>`: Frame rate for movement, pulse, and ordinary captured frames. Higher values create smoother but larger GIFs.
- `frameDelay=<milliseconds>`: Frame delay for movement, pulse, and ordinary captured frames. Must be `10..10000`; overrides `fps=`.
- `timeline=<true|false|file|directory>`: Default timeline behavior for nested recording blocks.

Action and nested block options override the surrounding `recording` defaults for that child only.

```text
gif "board evidence" pointerDuration=500 pointerEasing=ease-in-out pointerPath=arc {
  dragAndDrop ".todo-card" pointerDuration=1200 dragPath=manhattan dragTrail=true dragHold=250 {
    hover ".lane" pointerDuration=700
    moveMouse selector=".board" edge=bottom pointerDuration=300 pointerPath=direct dragBreadcrumbs=true
    drop ".done-column" dropPointerDuration=450 dragPath=arc postDropHold=800
  }
}
```

For block actions with child actions, the parent block is a scoped override and its children can still specify their own options. In the example above, the `gif` block sets the broad default, the `dragAndDrop` block overrides drag choreography for its body, and `hover`, `moveMouse`, and `drop` override individual movements.

`moveMouse` also accepts `duration=<milliseconds>` and `easing=<mode>` aliases:

```text
moveMouse selector=".board" edge=bottom duration=300 easing=linear pointerPath=manhattan
```

Add `holdAfterMove=<milliseconds>` when a recorded `moveMouse` should visibly settle before the next step:

```text
moveMouse selector=".board" edge=bottom duration=300 holdAfterMove=600
```

`dragAndDrop` supports drag-specific choreography:

- `pointerDuration=<milliseconds>`: Drag travel duration.
- `sourcePointerDuration=<milliseconds>`: Move-to-source duration before dragging.
- `targetPointerDuration=<milliseconds>`: Drag travel duration to the target.
- `dragEasing=<mode>`: Easing for drag travel.
- `dragPath=<direct|arc|manhattan|avoid-target|avoid-center>`: Route for held-pointer drag travel.
- `pressedPointer=<true|false>`: Keep the virtual pointer visually pressed while the drag is active. Defaults to `true`.
- `dragTrail=<true|false>`: Draw a line behind held-pointer drag movement.
- `dragBreadcrumbs=<true|false>`: Drop small dots along held-pointer drag movement.
- `preDragHold=<milliseconds>`: Hold before starting the page drag.
- `dragHold=<milliseconds>`: Hold while the drag is active before dropping.
- `postDropHold=<milliseconds>`: Hold after the drop pulse.
- Child `hover`, `moveMouse`, `delay`, and `drop` actions can override `pressedPointer=`, `dragTrail=`, and `dragBreadcrumbs=`. Child `drop` actions can also use `dropPointerDuration=<milliseconds>`, `dragPath=<...>`, and `postDropHold=<milliseconds>`.

Command-level defaults are available for whole-run recordings:

```powershell
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif --pointer-duration 600 --pointer-speed slow --pointer-easing spring
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif --pointer-theme branded --pointer-color "#2563eb" --pointer-size 40 --pointer-shadow strong
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif --caption-style teaching --caption-position bottom --caption-severity warning
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif --click-pulse ripple
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif --pointer-pre-click-hold 120 --pointer-post-click-hold 450
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif --gif-hold-on-failure 1800
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif --gif-fps 20
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif --gif-frame-delay 80
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif --gif-timeline demo-output\timelines
cmg run tests\flows --gif demo-output\runner-gifs --pointer-duration 600 --pointer-easing ease-in-out --pointer-theme ring --pointer-color "#dc2626" --caption-style qa --caption-position bottom --click-pulse dot --gif-hold-after-action 700 --gif-hold-on-failure 1800
cmg run tests\flows --gif demo-output\runner-gifs --gif-fps 20
cmg run tests\flows --gif demo-output\runner-gifs --gif-frame-delay 80
cmg run tests\flows --gif demo-output\runner-gifs --gif-timeline demo-output\timelines
cmg run tests\flows --gif demo-output\runner-gifs --gif-warn-size 500KB
cmg run tests\flows --gif demo-output\runner-gifs --gif-max-size 2MB
cmg run tests\flows --gif demo-output\runner-gifs --gif-max-duration 10s
```

CMG also enables evidence-focused defaults when they make the GIF easier to understand. Click and tap actions show a visible pulse by default so the recording proves that an activation happened. Right-clicks default to a crosshair pulse, middle-clicks default to a dot pulse, and double-clicks capture two pulse frames so the GIF shows both activations. Use `clickPulse=` when a script needs a different pulse style or needs to suppress the pulse for one action.

Automatic captions are opt-in because narration changes visual intent. CMG uses privacy-safe defaults: `fill` and `type` identify the target without copying the entered value into the GIF. With `captionPosition=auto`, captions appear below targets in the upper viewport half and above targets in the lower half. They exist only while recording; without `--gif` or a recording block, they do not touch the page.

```text
gif "guided checkout" autoCaptions=true captionPosition=auto captionStyle=teaching {
  fill "getByLabel=Cardholder name" "Ada Lovelace"
  click "getByRole=button|Pay" captionTemplate="{action}: {selector}"
  expectText "#status" "Paid"
}
```

Use scoped bookend cards or explicit chapter cards:

```text
gif "release flow" intro="Release verification" outro="Verification complete" introDuration=900 outroDuration=1100 {
  click "#deploy"
  intro "Post-deploy checks" duration=700
  expectText "#status" "Healthy"
  outro "Ready to share" duration=800
}
```

Title cards capture without the virtual pointer. Explicit actions skip with `status=skipped reason=no-active-recording` when no GIF is active.

Use timeline blocks to remove or re-time sections without changing browser behavior:

```text
gif "concise setup" {
  hideFromGif {
    click "#seed-data"
    waitForText "#status" "Ready"
  }
  speedUpGif factor=4 { pauseGif 1200 }
  slowDownGif factor=2 { click "#important" }
}
```

`hideFromGif` and `cutGif` suspend the active recorder, including nested GIF files and virtual-pointer injection. `speedUpGif` and `slowDownGif` scale frame delays from `0 < factor <= 100`; nested factors compose. No block changes real browser wait duration or event ordering. Without active recording, children still execute and no pointer is created.

```text
gif "click evidence" clickPulse=ripple {
  click "#save"
  rightClick "#menu"
  click "#tab" button=middle
  doubleClick "#open"
  click "#quiet" clickPulse=none
}
```

Use `pauseGif <milliseconds>` to add a recording-only hold without sleeping the browser or changing page state:

```text
gif "checkout evidence" holdAfterAction=600 {
  click "#pay"
  pauseGif 1200
  expectText "#status" "Paid"
}
```

Recording-only actions capture frames only when GIF recording is active. Without `--gif` or an active `gif` / `recordVideo` / `screencast` block, they are no-ops and report a skipped status. They do not create or move the virtual pointer outside GIF recording, and recording-only arguments, variables, scoped selectors, options, or child bodies are ignored because no recording exists to apply them to. Inside an active recording, the same actions validate their normal arguments and reject unsupported block bodies.

- `pauseGif <milliseconds>` reports `GIF_PAUSE <line> status=skipped reason=no-active-recording`.
- `moveMouse ...` reports `GIF_MOVE_MOUSE <line> status=skipped reason=no-active-recording`.
- `recordCheckpoint "<name>"` reports `GIF_CHECKPOINT <line> status=skipped reason=no-active-recording`.
- `showPointer` reports `GIF_SHOW_POINTER <line> status=skipped reason=no-active-recording`.
- `hidePointer` reports `GIF_HIDE_POINTER <line> status=skipped reason=no-active-recording`.
- Inside block `dragAndDrop`, choreography-only children also skip without an active recorder: `delay` reports `GIF_DRAG_DELAY <line> status=skipped reason=no-active-recording`, `hover` reports `GIF_DRAG_HOVER <line> status=skipped reason=no-active-recording`, `moveMouse` keeps the normal `GIF_MOVE_MOUSE` skipped line, `pauseGif` keeps the normal `GIF_PAUSE` skipped line, `recordCheckpoint` keeps the normal `GIF_CHECKPOINT` skipped line, `showPointer` keeps the normal `GIF_SHOW_POINTER` skipped line, and `hidePointer` keeps the normal `GIF_HIDE_POINTER` skipped line. Prep children such as `scrollIntoView` and `waitForElement` still run before the fallback native drag.

Use `recordCheckpoint "<name>"` to add a named marker to timeline JSON without adding a frame:

```text
gif "checkout evidence" timeline=true {
  recordCheckpoint "before payment"
  click "#pay"
  recordCheckpoint "after payment"
}
```

Use `showPointer` and `hidePointer` to capture pointer visibility changes without changing page state:

```text
gif "unobstructed state" {
  click "#open"
  hidePointer
  pauseGif 600
  showPointer
  click "#close"
}
```

Failure holds make failed artifacts easier to inspect:

```text
gif "failure evidence" holdOnFailure=1800 {
  click "#save"
  expectText "#status" "Saved"
}
```

When the wrapped block, direct script, or test fails, CMG captures one extra final-state frame before writing the partial GIF. This only happens when a GIF recorder is active; non-GIF runs do not inject the virtual pointer or capture failure frames.

## `moveMouse`

`moveMouse` is a script-only recording action. It has no one-off CLI equivalent. When no GIF recorder is active, it skips with `GIF_MOVE_MOUSE <line> status=skipped reason=no-active-recording` instead of creating or moving the virtual pointer. Skipped `moveMouse` actions do not expand variables or validate recording-only targeting options.

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
GIF_TIMELINE C:\Projects\CMG\demo-output\flow.timeline.json
```

On failure, CMG still writes a partial GIF when at least one frame was captured, then exits with code `1`.

## Timeline Metadata

Timeline metadata is optional and is written only when a recording is active.

- Direct scripts use `--gif-timeline <file-or-directory>` with command-level `--gif`.
- Runner tests use `cmg run --gif <directory> --gif-timeline <directory>`.
- Recording blocks use `timeline=true`, `timeline=false`, `timeline="artifacts/timelines"`, or `timeline="artifacts/flow.timeline.json"`.

When command-level `--gif` is active, nested `gif`, `recordVideo`, and `screencast` blocks are suppressed, including their block-level `timeline=` files. The command-level recorder writes one whole-run timeline instead.

The JSON sidecar contains:

- `version`, `createdAtUtc`, `gifPath`, `fileSizeBytes`, `quality`
- `frameCount`, `durationMilliseconds`, `width`, `height`
- `frameDelaysMilliseconds`
- `checkpoints`: named markers with `name`, `lineNumber`, `frameIndex`, and `timeMilliseconds`
- `timing.pointerDurationMilliseconds`, `timing.pointerSpeed`, `timing.pointerEasing`, `timing.clickPulse`, `timing.holdAfterActionMilliseconds`, `timing.preClickHoldMilliseconds`, `timing.postClickHoldMilliseconds`, `timing.holdAfterNavigationMilliseconds`, `timing.holdAfterAssertionMilliseconds`, `timing.holdOnFailureMilliseconds`

Use this file when reports, CI artifacts, or agent feedback need machine-readable timing without parsing the GIF binary. JSON run reports also include a `gifMetadata` array for recorded artifacts. Each entry contains the GIF path, quality preset when CMG knows it, frame count, duration, approximate FPS, dimensions, file size, palette color pressure, transparency, and repeat metadata. HTML run reports show recorded GIF artifacts as inline thumbnail previews with links to the original files. JUnit reports add `cmg.gif.path` properties for recorded artifacts; failed tests with an inspectable GIF also include `cmg.gif.failureFrameIndex`, currently the final frame index after the failure-state hold.

Use `cmg gif inspect <file>` when an agent needs to inspect an existing GIF artifact without rerunning the browser flow. It reports frame count, duration, dimensions, file size, transparency, repeat metadata, and palette color pressure as a parseable `GIF_INSPECT` line.

Use `cmg gif storyboard <file> --output <png>` when a reviewer or agent needs a still-image overview of a GIF. It samples frames into a PNG contact sheet and emits `GIF_STORYBOARD input="<gif>" output="<png>" frames=<exported>/<total> columns=<count> width=<pixels> height=<pixels>`.

Use `cmg gif optimize <file> --output <gif>` to coalesce consecutive duplicate frames in an existing artifact while preserving duration. It emits `GIF_OPTIMIZE input="<gif>" output="<gif>" framesBefore=<count> framesAfter=<count> duplicateFramesRemoved=<count> durationMs=<milliseconds> sizeBeforeBytes=<bytes> sizeAfterBytes=<bytes>`.

Use `cmg run --gif-warn-size <size>` when CI or agents should flag unexpectedly large visual artifacts. The runner emits `GIF_WARN_SIZE test="<name>" path="<gif>" sizeBytes=<bytes> thresholdBytes=<bytes>` after the relevant test result line, and the warning does not fail the run.

Use `cmg run --gif-max-size <size>` when CI should fail visual evidence that is too large to upload or review. The runner emits `GIF_MAX_SIZE test="<name>" path="<gif>" sizeBytes=<bytes> thresholdBytes=<bytes>`, marks that test failed, writes the reason into reports, and exits `1`.

Use `cmg run --gif-max-duration <duration>` when CI should fail visual evidence that is too long to review. Durations accept plain milliseconds or `ms`, `s`, and `m` suffixes. The runner emits `GIF_MAX_DURATION test="<name>" path="<gif>" durationMs=<milliseconds> thresholdMs=<milliseconds>`, marks that test failed, writes the reason into reports, and exits `1`.

Palette pressure diagnostics are automatic for runner GIF artifacts. When a recorded GIF uses at least 240 decoded colors, or the inspector reaches the `>256` color cap, the runner emits `GIF_WARN_PALETTE test="<name>" path="<gif>" paletteColors=<count-or->256> thresholdColors=240 palette=<mode>`. This warning does not fail the run; it tells agents and CI that the GIF may show color loss or visible dithering and may need quality, crop, or future format tuning.

## Quality

GIF quality controls palette generation and dithering. It does not change the browser screenshot source, virtual pointer, pointer events, drag ghosts, captions, timing, or which frames are captured.

- `highest`: Default. Uses CMG's most color-faithful palette matching and dithering for visual evidence.
- `high`: Still uses a full 256-color palette with slightly lighter dithering.
- `medium`: Uses a smaller palette and faster matching for smaller artifacts.
- `low`: Uses a much smaller palette with no dithering for the smallest/fastest artifacts.

Use command-level quality for whole-run recordings:

```powershell
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif --gif-quality highest
cmg run flow.cmgscript --gif demo-output\runner-gifs --gif-quality high
```

Use block-level `quality=` for script-level recording blocks and aliases:

```text
gif "checkout" quality=highest {
  click "#checkout"
}

recordVideo "hover-state" quality=high {
  hover "#menu"
}

screencast "compact" quality=medium {
  click "#open"
}
```

When command-level `--gif` is active, nested `gif`, `recordVideo`, and `screencast` block files are still suppressed and the command-level `--gif-quality` applies to the whole recording.

## Timing

CMG uses balanced default timing:

- about 10 frames per second
- smooth pointer movement between targets, configurable with `pointerDuration=`, `pointerSpeed=`, and `pointerEasing=`
- short holds after actions
- a visible click pulse for click and drag/drop actions

Timing is automatic by default. Use pointer choreography options when a GIF needs slower evidence movement, instant jumps, or action-specific drag timing.

## Notes

- GIF files can become large for long scripts.
- Recording can make scripts run slower because screenshots are captured after each action.
- GIF recording is supported by `browser control script --gif`, `cmg run --gif`, and `gif "name" { ... }` blocks inside direct browser-control scripts or `cmg run` scripts.
- `cmg run --gif` prefixes runner GIF filenames with the selected `--project` name, so matrix jobs can write Chrome, Firefox, and Edge artifacts into the same directory without overwriting each other.
- `recordVideo "name" { ... }` and `screencast "name" { ... }` are provider-style aliases for CMG GIF blocks. They write animated GIF files, not MP4/WebM files.
- Command-level `--gif` records the whole direct script or test and suppresses nested `gif` block files. Suppressed blocks write `GIF_BLOCK_SUPPRESSED <line>` to stdout.
- GIF-only timeline and pointer-visibility actions such as `moveMouse`, `pauseGif`, `recordCheckpoint`, `showPointer`, and `hidePointer` skip when no recording is active; scripts without recording do not create or move a virtual mouse.
- Failed GIF recordings hold the final visible state for `holdOnFailure` / `--gif-hold-on-failure` milliseconds before the partial GIF is written.
