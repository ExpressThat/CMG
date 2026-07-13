# GIF Recording Improvements

This is a backlog of ideas for making CMG's GIF recording best in class. The goal is not merely to export an animation, but to produce choreographed, human-readable, pointer-accurate visual evidence for browser automation.

## Design Principle: DSL First, CLI Second

Recording behavior should be designed for the CMG DSL first. The DSL is where users and AI agents choreograph intent: what to show, what to hide, how to move, where to pause, what to caption, and what evidence matters.

CLI flags should exist only when they make sense as whole-run defaults, CI controls, or simple one-shot convenience settings. A good rule:

- Use DSL actions, block options, and scoped `recording { ... }` settings for choreography and per-section intent.
- Use CLI options for coarse defaults such as output directory, default quality, record-on-failure, max artifact size, and CI retention.
- If an option only makes sense for one part of a flow, it belongs in the DSL, not the CLI.
- If a CLI option exists, there should usually be an equivalent or more expressive DSL form.
- `gif`, `recordVideo`, and `screencast` aliases should share every recording option unless a future output format makes a distinction necessary.

Some improvements should become defaults when they make the recorded evidence clearer without requiring extra user intent. Defaults should favor showing what actually happened in a human-readable way, while still letting scripts override or disable the behavior. For example, clicks should visibly pulse by default because the GIF should prove a click happened; `clickPulse=none` or a scoped default can opt out when a pulse would be distracting.

Default-first guidance:

- Prefer defaults for evidence features that nearly every recording benefits from, such as visible click indication, readable pointer movement, enough post-action hold time to see state changes, and failure-state holds.
- Prefer options for features that change intent, privacy, output size, or visual style, such as masking, cropping, captions, pointer speed, palette tuning, or artifact retention.
- If a feature becomes a default, it still needs a DSL override and, when useful for whole-run behavior, a CLI override.
- Default behavior must be documented as part of the feature, including how to disable it. For example: `clickPulse=ring` by default, `clickPulse=none` to suppress click evidence in a specific block or action.
- Defaults should be conservative and evidence-oriented: they should reveal automation intent without inventing actions, hiding failures, or making the GIF feel decorative.

Preferred examples:

```text
recording quality=highest pointerSpeed=slow captionStyle=qa {
  gif "checkout" crop="#checkout-panel" {
    click "#pay"
    expectText "#status" "Paid"
  }
}

gif "drag evidence" pointerSpeed=slow easing=ease-in-out holdAfterAction=600 {
  dragAndDrop ".card" ".done"
}
```

CLI equivalents should be secondary, for example:

```powershell
cmg run tests --gif artifacts\gifs --gif-quality highest --gif-on-failure
```

## Option Applicability Model

Every recording option should declare where it applies. Avoid designing a knob as only a CLI flag or only a GIF-block flag when it is also useful as a scoped default or per-action override.

Recommended precedence, from lowest to highest:

1. Project/config default.
2. CLI whole-run default.
3. DSL `recording { ... }` scoped default.
4. Recording block option on `gif`, `recordVideo`, or `screencast`.
5. Action-level option.
6. Child action-specific override inside a complex action such as `dragAndDrop { ... }`.

Example:

```text
recording pointerDuration=450 pointerEasing=ease-in-out {
  gif "checkout" pointerDuration=650 {
    click "#plan-pro"
    click "#continue" pointerDuration=250
  }
}
```

In that example:

- `recording pointerDuration=450` is the default for the scoped section.
- `gif "checkout" pointerDuration=650` overrides the scoped default for actions inside that GIF block.
- `click "#continue" pointerDuration=250` overrides both for one action.

Complex actions need two layers of control:

```text
gif "kanban drag" pointerDuration=500 {
  dragAndDrop ".todo-card" ".done-column" pointerDuration=1200 dragHold=300 dropHold=700 {
    moveMouse selector=".board" edge=bottom pointerDuration=800
    delay 1000
    drop ".done-column" pointerDuration=400
  }
}
```

Here `gif pointerDuration=500` controls ordinary pointer moves inside the block, while `dragAndDrop pointerDuration=1200` controls the drag travel itself. Child actions inside the drag body can still override their own movement.

For any block action that has child actions, the parent block's recording options are scoped defaults for everything inside the block. A child action can specify the same option to override the parent for that child only. This applies to top-level recording blocks such as `gif { ... }` and complex action blocks such as `dragAndDrop { ... }`.

Example:

```text
gif "board evidence" pointerDuration=500 pointerEasing=ease-in-out {
  dragAndDrop ".todo-card" pointerDuration=1200 dragHold=250 {
    hover ".lane" pointerDuration=700
    moveMouse selector=".board" edge=bottom pointerDuration=300
    drop ".done-column" dropPointerDuration=450 postDropHold=800
  }
}
```

In that example:

- The `gif` block sets defaults for all recorded child actions.
- The `dragAndDrop` block overrides drag choreography defaults for its body.
- `hover`, `moveMouse`, and `drop` can each override the parent block defaults for their own movement and holds.

Use this table when designing each feature:

| Setting Type | Project / Config | CLI Whole Run | `recording {}` Scope | `gif` / `recordVideo` / `screencast` Block | Action Override | Complex Child Override |
| --- | --- | --- | --- | --- | --- | --- |
| Output path/directory | yes | yes | sometimes | yes | no | no |
| Quality/format/codec | yes | yes | yes | yes | rarely | no |
| Pointer speed/duration/easing | yes | yes | yes | yes | yes | yes |
| Holds and pauses | yes | yes | yes | yes | yes | yes |
| Evidence defaults such as click pulse | yes | yes | yes | yes | yes | yes |
| Caption style/position/template | yes | yes | yes | yes | yes | yes |
| Crop/scale/viewport | yes | yes | yes | yes | rarely | no |
| Redaction/masking | yes | yes | yes | yes | yes | sometimes |
| Debug overlays | yes | yes | yes | yes | yes | yes |
| Retention/on-failure policy | yes | yes | no | no | no | no |
| Timeline export/report integration | yes | yes | yes | yes | no | no |

For each backlog item below, prefer documenting all sensible levels explicitly.

## Recording Quality And Color

- Implemented: DSL `quality=<highest|high|medium|low>` presets on `gif`, `recordVideo`, and `screencast`.
- Implemented: DSL `recording quality=highest { ... }` scoped defaults and CLI `--gif-quality <highest|high|medium|low>` as a whole-run default.
- Implemented: lossless-ish `quality=archival` uses a 256-color frame-local palette and maximum preset dithering strength. CLI: `--gif-quality archival`.
- Implemented: `dither=<none|floyd-steinberg|bayer|atkinson|sierra>` and whole-run CLI `--gif-dither <mode>`.
- Implemented: `palette=<global|local|adaptive>` and whole-run CLI `--gif-palette <mode>`; adaptive currently selects frame-local tables.
- Implemented: `colors=<2..256>` and whole-run CLI `--gif-colors <count>`.
- Implemented: add automatic runner palette diagnostics that emit `GIF_WARN_PALETTE` when recorded artifacts approach the GIF color limit.
- Add DSL `format=<gif|apng|webp|mp4>` on recording blocks while keeping GIF as the default visual-evidence format.
- Add CLI `--record-format <gif|apng|webp|mp4>` only for whole-run output defaults.
- Add APNG output for lossless color when users do not need old GIF compatibility.
- Add animated WebP output for smaller files with better color.
- Add MP4/H.264 or WebM export for long recordings where GIF is too large.
- Implemented for DSL recording blocks: sidecar PNG frame export preserves final pre-quantization frames; without crop, scale, or color transforms these remain the exact browser PNG bytes.
- Implemented: DSL `keepFrames=<directory|true|false>` and whole-run CLI `--keep-frames <directory>`. Runner output is isolated per GIF for parallel safety.
- Implemented: `cmg gif color-diff <source.png> <encoded.gif> --frame <n>` reports parseable MAE, RMS, maximum-channel, and changed-pixel encoder drift.
- Implemented for gradients, exact source-byte retention, transparent-background flattening, and color-metadata transitions: automated color tests. Broader browser shadow baselines remain open.
- Implemented: direct and structured-runner gradient/brand-color fidelity fixtures in demos 165 and 166. Broader browser visual baselines remain open.
- Implemented: inherited `background=<color|transparent|none>` and CLI `--gif-background <color>` flatten transparent captures before quantization.
- Implemented: inspect ICC, CICP, and gamma metadata on browser PNGs; timeline/stats expose counts and profile changes emit `GIF_WARN_COLOR_PROFILE`.
- Implemented: inherited `highContrastPalette=<true|false>` and CLI `--gif-high-contrast-palette` deliberately increase contrast and saturation for accessibility review.
- Implemented: `dither=none` for sharp UI screenshots where dithering looks noisy.
- Implemented: inherited `gradientMode=<smooth|text>` and CLI `--gif-gradient-mode <smooth|text>` select useful defaults while explicit controls remain authoritative.

## Virtual Pointer Choreography

- Implemented: pointer speed presets and positive multipliers inherit through recording scopes/blocks and override on pointer-aware and drag child actions; CLI `--pointer-speed` sets whole-run defaults.
- Implemented: pointer duration works at recording, block, action, drag source/target/drop, and child levels; `moveMouse duration=` aliases `pointerDuration=` and CLI `--pointer-duration` sets the whole run.
- Implemented: linear, ease-in, ease-out, ease-in-out, and spring easing works at recording, action, moveMouse alias, and independent drag levels; CLI `--pointer-easing` sets the whole run.
- Implemented: add pointer path styles: direct line, curved arc, Manhattan path, avoid-target, and avoid-center.
- Implemented: add `pointerPath=<direct|arc|manhattan|avoid-target|avoid-center>` on recording blocks and pointer-aware actions.
- Implemented: add `dragPath=<direct|arc|manhattan|avoid-target|avoid-center>` on `dragAndDrop`.
- Add auto pathing that avoids covering the target text or button label.
- Implemented: add `preClickHold=<ms>` and `postClickHold=<ms>` at recording-block and action level. CLI: `--pointer-pre-click-hold <ms>` and `--pointer-post-click-hold <ms>`.
- Implemented: add `preDragHold=<ms>`, `dragHold=<ms>`, and `postDropHold=<ms>` on `dragAndDrop`.
- Implemented: add `holdAfterMove=<ms>` on `moveMouse` for demonstrations where the pointer should settle before the next action.
- Implemented: click pulse styles ring, ripple, dot, crosshair, and none inherit through recording scopes and action overrides; CLI `--click-pulse` defaults to ring.
- Implemented: add right-click and middle-click distinct visual pulses.
- Implemented: add double-click pulse choreography that clearly shows two clicks.
- Implemented: add drag trail rendering for long drags.
- Implemented: add optional drag path breadcrumb dots.
- Implemented: `mouseDown` captures a pressed pointer only after the real down event; `mouseDownHold=` and `--mouse-down-hold` default to 500ms.
- Implemented: add a pressed pointer state while dragging.
- Implemented: add touch pointer styling for `tap` and `touchTap`.
- Implemented: focus-producing keyboard/pointer actions pulse the actual focused control by default; `focusPulse=false` and `--no-pointer-focus-pulse` opt out.
- Implemented: `targetCallout=<auto|always|none>` outlines and calls out active targets, with `targetCalloutThreshold=` defaulting to 24px; whole-run CLI controls are available.
- Implemented: add a configurable pointer theme: system arrow, hand, dot, ring, touch, or branded pointer. CLI: `--pointer-theme <theme>`.
- Implemented: add pointer color and size options. CLI: `--pointer-color <color>` and `--pointer-size <pixels>`.
- Implemented: add pointer shadow strength options for dark/light pages. CLI: `--pointer-shadow <none|light|medium|strong>`.
- Implemented: uncolored pointers automatically choose foreground/edge contrast against the page; `pointerContrast=fixed` and `--pointer-contrast fixed` opt out.
- Implemented: add `showPointer=<true|false|auto>` for recording scopes, recording blocks, and pointer-aware action overrides. CLI: `--show-pointer <true|false|auto>`.
- Implemented: long holds use a three-stage pointer halo without changing encoded duration; `pointerIdle`, `pointerIdleThreshold`, and whole-run CLI equivalents control it.
- Implemented: instant/zero-duration movement shows origin and dashed travel evidence by default; `teleportMarker=false` and `--no-pointer-teleport-marker` opt out.

## Captions And Narration

- Implemented: add caption style presets: subtle, teaching, QA evidence, bug report, compact.
- Implemented: add `captionStyle=<subtle|teaching|qa|bug-report|compact>` on recording blocks, `step`, and captions. CLI: `--caption-style <style>`.
- Implemented: add `captionPosition=<top|bottom|left|right|auto>` on recording blocks, `step`, and captions. CLI: `--caption-position <position>`.
- Implemented for automatic action captions: `captionPosition=auto` places the caption opposite the active target's viewport half. Target-aware placement for standalone manual captions remains open.
- Implemented in DSL recording scopes and recording blocks: `autoCaptions=true`. A whole-run CLI default remains open.
- Implemented in DSL recording scopes, recording blocks, and action overrides: `captionTemplate=` with `{action}`, `{selector}`, `{target}`, `{line}`, and `{arguments}`. A whole-run CLI default and structural `{step}` / `{assertion}` context placeholders remain open.
- Implemented: `caption duration=<ms>` / `captionDuration=<ms>` controls encoded visibility duration and can be inherited from recording scopes.
- Implemented: `fadeIn=<ms>` and `fadeOut=<ms>` capture deterministic opacity stages and remove completed captions.
- Implemented: nested `step` captions stack as breadcrumbs and restore parent context; `captionStacking=false` opts out.
- Implemented: `persistentStepTitle=true` captures and restores scoped step titles.
- Implemented: `sourceLineCaptions=true` includes DSL line numbers in step and debug narration.
- Implemented: automatic bounded failure captions with `failureCaptions=false` opt-out and parseable `GIF_FAILURE_CAPTION` diagnostics.
- Implemented: automatic assertion captions show expected vs actual values, mask sensitive values, and support `assertionCaptions=false` opt-out.
- Implemented: add privacy-safe network captions for request/response waits through `networkCaptions=true` and `--gif-event-captions`.
- Implemented: add dialog setup and observed-result captions through `dialogCaptions=true`.
- Implemented: add console/page-error capture, count, success, and observed-event captions through `consoleCaptions=true`.
- Implemented: add caption severity styles: info, success, warning, error. CLI: `--caption-severity <severity>` for whole-run defaults.
- Implemented: generated pass/failure/expected/actual labels have inherited localization hooks.
- Implemented: `captionFormat=markdown` safely renders bold and inline-code spans with DOM nodes, never caller HTML.
- Implemented: nestable `narrate "message" { ... }` teaching-style blocks with scoped execution context and timing options.
- Implemented: `debugNarration=true` captions `try`, `catch`, and `finally` transitions.
- Implemented: `debugNarration=true` captions loop iterations and macro calls.

## Timeline And Editing Controls

- Implemented: add `fps=<number>` on recording scopes for output frame rate. CLI: `--gif-fps <number>`.
- Implemented: add `frameDelay=<ms>` for advanced timing. CLI: `--gif-frame-delay <ms>`. Explicit `frameDelay` overrides `fps`.
- Implemented: `holdAfterAction=<ms>` as a block/action default and CLI `--gif-hold-after-action <ms>` for whole-run recordings.
- Implemented: `pauseGif <ms>` as a recording-only hold action that captures a timed GIF frame when recording is active and skips without injecting the virtual pointer when no GIF recorder is active.
- Implemented: add `holdAfterNavigation=<ms>`. CLI: `--gif-hold-after-navigation <ms>`.
- Implemented: add `holdAfterAssertion=<ms>`. CLI: `--gif-hold-after-assertion <ms>`.
- Implemented: `holdOnFailure=<ms>` so final failure state is readable. CLI: `--gif-hold-on-failure <ms>`.
- Implemented: add DSL intro actions/defaults plus whole-run `--gif-intro` and `--gif-intro-duration` controls.
- Implemented: add DSL outro actions/defaults plus whole-run `--gif-outro` and `--gif-outro-duration` controls.
- Implemented: add `resultOutro=true` and `--gif-result-outro` outcome cards for passed, failed, and skipped runs, with explicit outro precedence.
- Implemented: `pauseGif <ms>` is a recording-only hold and safely skips without a recorder.
- Implemented: add `cutGif { ... }` to execute boring waits/setup while suppressing their frames, pointer, captions, checkpoints, and nested GIF artifacts.
- Implemented: add nestable `speedUpGif factor=<number> { ... }` blocks for long setup sections.
- Implemented: add nestable `slowDownGif factor=<number> { ... }` blocks for important interactions.
- Implemented: add `hideFromGif { ... }` as the intent-revealing alias for cut sections.
- Implemented: `recordCheckpoint "name"` markers emit parseable step output and write frame/time bookmarks into GIF timeline JSON when recording is active.
- Implemented: add timeline metadata JSON beside each GIF. CLI: `--gif-timeline <file|directory>`; DSL blocks support `timeline=<true|false|file|directory>`.
- Implemented: `cmg gif trim` supports inclusive frame ranges and precise millisecond ranges with adjusted boundary delays.
- Implemented: add `cmg gif optimize <file> --output <gif>` to remove consecutive duplicate frames while preserving duration.
- Implemented: automatically coalesce exact consecutive pre-quantization frames while preserving total delay; `coalesceDuplicates=false` and `--gif-no-coalesce` opt out.
- Implemented: inherited long-wait threshold/duration controls compress visual evidence while labels preserve requested time and browser waits retain real runtime.
- Implemented: long waits show deterministic 33%, 67%, and 100% progress stages; `waitProgress=false` opts out.
- Implemented: add zero-based frame/time spans for nested and repeated runtime steps plus final failure-frame bookmarks in timeline schema version 2.
- Implemented: add `cmg gif storyboard <file> --output <png>` to export GIF frames as a contact sheet PNG.
- Implemented: add self-contained static step-start and final-failure frame evidence in HTML reports, linked from the runtime step table.

## Viewport, Layout, And Framing

- Implemented: inherited `viewport=<width>x<height>` and CLI `--gif-viewport` temporarily set recording dimensions and restore the previous viewport.
- Implemented: `scale=<0.05..1>` downscales complete composited frames. CLI: `--gif-scale <number>`.
- Implemented: `maxWidth=` and `maxHeight=` preserve aspect ratio. CLI: `--gif-max-width <pixels>` and `--gif-max-height <pixels>`.
- Implemented: inherited `pixelRatio=<1..4>` and CLI `--gif-pixel-ratio` provide controlled high-DPI capture.
- Implemented: `crop="<selector-or-rich-locator>"` resolves live for every frame. CLI: `--gif-crop <selector>`.
- Implemented: `gif "name" crop="#panel" { ... }` block support, with `cropPadding=` / `--gif-crop-padding` for context.
- Add smart crop that follows the pointer and active element.
- Add split-screen recording for multi-tab or popup flows.
- Add frame recording inside iframes with visible page context.
- Implemented: active recorders center pointer-targeted elements before coordinate resolution; non-recorded scripts are unchanged.
- Add safe-area padding so pointer/captions are not clipped.
- Implemented in the DSL: blur, solid, or replacement masks on recording scopes/blocks and individual actions. CLI repeatable `--gif-mask` / `--gif-blur` coarse defaults remain open.
- Implemented: `maskGif` / `redactGif` / `redactText` and `unmaskGif` / `unredactGif` recording-only actions with live locator resolution.
- Add redaction presets for emails, tokens, passwords, and credit-card-like text.
- Add live element highlight outlines that follow the target through the action.
- Add target zoom inset, like a magnifying glass, for tiny controls.
- Add mini-map or viewport position indicator for long pages.
- Add automatic viewport stabilization after layout shifts.
- Add handling for sticky headers that would cover scrolled targets.

## Script And DSL Controls

- Implemented: recording options on `gif`, `recordVideo`, and `screencast`, including `quality=`, `fps=`, `pointerSpeed=`, `captionStyle=`, `crop=`, and `scale=`.
- Implemented: add `recording { ... }` block for shared recording settings and make it inherit through nested macros, loops, `try/catch`, `within`, frame scopes, nested recording blocks, and command-level `--gif` action choreography.
- Implemented: `setRecording quality=highest pointerSpeed=normal` mutates subsequent defaults in the current lexical scope and reports the complete effective settings.
- Implemented: add scoped recording settings that restore after the block.
- Implemented: add `withRecording quality=highest pointerDuration=500 { ... }` as a readable alias if `recording { ... }` feels too much like it must record by itself.
- Implemented in the DSL: `recordingDefaults { ... }` is a scoped house-style block. Folder/project configuration remains open.
- Add action-level recording overrides consistently:
  - `click "#save" pointerDuration=250 clickPulse=ripple`
  - `hover "#menu" pointerDuration=700 postHoverHold=500`
  - `fill "#name" typingDelay=50 pointerDuration=300`
  - `wheel "#list" pointerDuration=250 holdAfterAction=400`
  - `dragAndDrop ".card" ".done" pointerDuration=1200 dragEasing=ease-in-out`
- Add nested complex-action overrides consistently:
  - `dragAndDrop { moveMouse ... pointerDuration=800; drop ... pointerDuration=400 }`
  - future multi-step gestures should follow the same child override model.
- Add `gif.ifChanged "name" { ... }` to write only when visual changes occurred.
- Add `gif.onFailure "name" { ... }` to record only diagnostic sections when a flow fails.
- Add `gif.snapshot "name"` to capture a short still/hold frame sequence.
- Add `annotateTarget "#save" "Primary action"` action.
- Implemented: add `showPointer` and `hidePointer` recording-only actions that skip when no recorder is active, including inside block `dragAndDrop` choreography.
- Add `pointerStyle` action to change pointer appearance mid-script.
- Add `highlightTarget` action that coordinates highlight and pointer move.
- Implemented: add `showKeystrokes` block for keyboard-heavy flows. It is a recording-default scope, skips visual injection without an active recorder, and never renders typed values.
- Add `showMouseButtons` overlay for low-level mouse scripts.
- Add `showNetworkActivity` overlay for network-heavy scripts.
- Add `showConsoleActivity` overlay for console/page-error diagnostics.
- Add `recordVariable "name"` caption action for explaining state changes.
- Add dry-run validation that reports recording settings without launching the browser.

## Test Runner And Reports

- Add per-test GIF quality in runner declarations: `test "x" gifQuality=high`.
- Add suite-level GIF defaults: `describe "x" gifQuality=highest`.
- Add config-file support for GIF quality, pointer speed, FPS, and crop settings.
- Add project-level GIF settings for cross-browser matrices.
- Implemented: add GIF metadata to JSON reports: quality, approximate FPS, frame count, duration, dimensions, palette details, transparency, repeat metadata, and file size.
- Implemented: add GIF thumbnail previews in HTML reports with artifact links.
- Implemented: add a static final failure-frame thumbnail in HTML reports.
- Implemented: add per-step GIF path, timeline path, frame spans, time spans, and failure-frame mappings in JSON reports.
- Implemented: add report links that jump to the embedded frame where a visual step started.
- Implemented: add report links that jump to the embedded final failure frame.
- Implemented: add JUnit properties for GIF paths and failed-test final-frame indexes.
- Implemented: add artifact size warnings in runner output. CLI: `--gif-warn-size <size>` emits `GIF_WARN_SIZE` for recorded GIFs over the threshold.
- Add runner declaration `gif=onFailure` to record or keep GIFs only for failing tests. CLI: `--gif-on-failure`.
- Add runner declaration `gif=onRetry` to keep GIFs for failed attempts only. CLI: `--gif-on-retry`.
- Add runner declaration `gifSampleRate=` for huge suites. CLI: `--gif-sample-rate <n>`.
- Add runner declaration `gifCleanPassed=true` to delete passed-test GIFs after report generation. CLI: `--gif-clean-passed`.
- Add CI summary page that groups GIFs by failure reason.
- Add test-run filmstrip report across all selected tests.
- Add shard-safe GIF naming with project/browser/retry/repeat metadata.

## Browser And Protocol Fidelity

- Capture Chrome/Edge screenshots through PNG by default before GIF encoding.
- Add Firefox-specific capture parity tests for color and pointer compositing.
- Add browser-specific calibration pages for screenshot color differences.
- Add detection for browser screenshot color profile changes.
- Add robust top-layer pointer handling for dialogs, popovers, fullscreen, and high-z-index overlays.
- Add pointer survival across navigation and reload.
- Add pointer survival across context switches and tabs.
- Add pointer survival across same-origin iframe actions.
- Add explicit cross-origin iframe recording diagnostics.
- Add service-worker/network overlay support when recording network flows.
- Add WebSocket event overlays.
- Add worker action overlays.
- Implemented as capture-only captions: show browser dialog capture and accepted/dismissed handling state without re-dispatching dialogs.
- Implemented for event evidence: show download completion without exposing its path. Report artifact links remain open.
- Implemented for event evidence: show selected upload file counts without exposing filenames or file contents.
- Add support for browser zoom level detection and correction.
- Add support for prefers-reduced-motion testing while still making GIFs understandable.
- Add support for pages using CSS transforms, zoom, and nested scrolling containers.
- Implemented: opt-in conservative cleanup for CMG-launched headless browsers tracks browser/port/PID/start-time/launch-token ownership, renews activity throughout control and runner operations, uses configurable renewable leases, warns and rechecks before expiry, leaves visible/attached/user-launched/replaced browsers alone, isolates concurrent ports, emits parseable persisted diagnostics, and supports launch/run/config/environment enablement plus keepalive/disable commands.

## Performance And Storage

- Add streaming GIF encoding to avoid holding every frame in memory.
- Add frame diffing to store only changed regions where the encoder supports it.
- Implemented: detect and coalesce exact duplicate frames before encoding and report source/retained counts.
- Implemented: add `sampleEvery=<1..100>` and `--gif-sample-every` for intermediate pointer/drag frames while preserving final semantic frames and all pointer events.
- Add parallel frame preprocessing before final encode.
- Implemented: emit peak retained RGBA pixel bytes and preprocessing time in `GIF_CAPTURE_STATS` and timeline `captureDiagnostics`.
- Implemented: add max duration guard with clear failure reason. CLI: `--gif-max-duration <duration>` emits `GIF_MAX_DURATION`, fails the test, and writes the reason into reports.
- Implemented: add max file size guard with clear failure reason. CLI: `--gif-max-size <size>` emits `GIF_MAX_SIZE`, fails the test, and writes the reason into reports.
- Add automatic downscale when file size exceeds a threshold.
- Add automatic quality fallback when file size exceeds a threshold.
- Add `sizeBudget=<size>` to target a file size. CLI: `--gif-budget <size>`.
- Add recording cache cleanup policy.
- Add artifact retention settings for CI. CLI: `--gif-retention <all|failed|none|days:n>`.
- Add per-step capture cost metrics.
- Implemented: add a `cmg gif inspect <file>` command for frame count, palette color pressure, duration, dimensions, transparency, repeat metadata, and size.
- Implemented: add a `cmg gif optimize <file> --output <gif>` command for duplicate-frame coalescing.
- Implemented: add a `cmg gif compare <before> <after>` command for frame, duration, dimension, palette, transparency, repeat, and size deltas.

## Debugging And Diagnostics

- Implemented: add `debug=true` on recording blocks and `--gif-debug` to write a `.debug.json` record for every captured frame.
- Implemented: add a capture-only HUD showing current action name and source line.
- Implemented: add nested macro/loop/step context to the diagnostics HUD and sidecar.
- Implemented: add current selector metadata and a resolved target rectangle.
- Implemented: add virtual-pointer coordinates to the diagnostics HUD and sidecar.
- Implemented: add scroll position to the diagnostics HUD.
- Add overlay showing active browser tab/context.
- Add warning when an action was non-visual and did not capture a meaningful frame.
- Add warning when pointer target is offscreen and had to be scrolled.
- Add warning when a selector resolved to multiple elements.
- Add warning when a target is too small to see clearly.
- Implemented: emit non-failing `GIF_WARN_BLANK` when retained evidence is mostly white, black, or transparent.
- Implemented: emit non-failing `GIF_WARN_UNCHANGED` when at least 60% of source frames are exact consecutive duplicates.
- Add diagnostics when the pointer could not be promoted above page UI.
- Add diagnostics when GIF blocks are suppressed by command-level recording.
- Add diagnostics for invalid recording settings in runner reports.
- Add a generated reproduction command for every GIF.

## Accessibility And Review Use Cases

- Implemented: add keyboard overlay for keyboard-only flows through `showKeystrokes=true` and the `showKeystrokes {}` scope.
- Implemented: add focus ring amplification for accessibility evidence through `focusEvidence=true`.
- Implemented: add ARIA role/name captions for targeted or focused controls through `accessibleNames=true`.
- Implemented: add `contrastWarnings=true` and `--gif-accessibility` target contrast warnings using the applicable `4.5:1` or `3:1` WCAG threshold.
- Implemented: add `reducedMotion=true` plus `--gif-reduced-motion`, with static pointer travel/click evidence, suppressed inherited caption fades, and child overrides.
- Implemented: add `highContrastPointer=true` plus `--gif-high-contrast-pointer`, with a large yellow ring, strong edge, and per-property child overrides.
- Implemented: add `captionSize=<normal|large|x-large>` on recording scopes and child captions, plus `--caption-size` for whole-run GIFs.
- Add screen-reader narration sidecar text file.
- Implemented: add an `accessibilityEvidence=true` preset that captures keyboard labels, focused-element evidence, accessible role/name callouts, high-contrast styling, and contrast warnings.
- Add alt text generation template for GIF artifacts.
- Add report field for human-written GIF description.
- Add option to export a step-by-step still image PDF for reviewers who cannot view GIFs.

## Privacy And Security

- Implemented: `redactText "<selector>"` action for frame-only GIF redaction.
- Implemented: automatic password input masking is the recording default.
- Implemented: `autoRedact=sensitive` adds automatic token-like text and value masking.
- Implemented in the DSL: semicolon-separated `redact="<selector>"` defaults and repeatable persistent redaction actions. CLI repeatable `--gif-redact <selector>` remains open as a coarse whole-run safety default.
- Add per-project redaction config.
- Implemented: timeline JSON includes redaction configuration and frame/time audit entries without secret content.
- Implemented: `redactionSafety=strict` refuses unsafe password-field capture without retrying an unsafe failure frame.
- Implemented: blur, solid mask, and replacement text redaction styles.
- Add option to hide URL bar or browser chrome if future capture includes chrome surfaces.
- Add environment-variable controlled recording disable switch for sensitive CI. CLI/env: `CMG_DISABLE_GIF=1` and `--no-gif`.

## Authoring Experience

- Implemented: `previewRecordingSettings` prints effective runtime DSL recording settings without requiring active capture.
- Implemented: `cmg browser control script --preview-gif-settings` performs browser-free static analysis of imports, syntax, scoped settings, and relevant warnings.
- Implemented: add `cmg gif presets` to list quality, pointer speed, pointer easing, click pulse, and timing presets.
- Add docs with side-by-side examples of quality presets.
- Add generated sample GIFs to docs for visual comparison.
- Add a quick-start command specifically for first GIF creation.
- Add inline suggestions when users use `recordVideo` expecting MP4.
- Add warnings that `recordVideo` and `screencast` are GIF aliases unless `format=` or a future whole-run format default changes that.
- Add VS Code snippets for GIF blocks and recording settings.
- Implemented: static preflight warns when explicit virtual-pointer/visual-only options are attached to non-visual actions.
- Add lint rules for overly long GIF blocks.
- Add lint rule suggesting `gif=onFailure` or the equivalent whole-run CLI default for large test suites.
- Add script style-guide section for readable visual evidence.

## Agent Browser Lifecycle Safety

- Implemented: cleanup is opt-in and never triggers merely because a command ended.
- Implemented: positive `ms`/`s`/`m`/`h` leases through `browser launch --idle-timeout`, `cmg run --browser-idle-timeout`, run config, and `CMG_BROWSER_IDLE_TIMEOUT`; disabled is the default.
- Cleanup is deliberately conservative: it is opt-in, applies only to CMG-owned headless browsers, and should use a generous timeout so an agent can reason, edit, or perform other work before returning. Visible browsers and headless browsers without a lease are never auto-closed.
- Implemented: control commands, direct scripts, and full runner operations renew the lease, including periodic heartbeats during long operations.
- Implemented: state tracks CMG ownership token, browser, port, PID, process start time, launch/activity times, and timeout; nonmatching or attached state is never closed.
- Implemented: graceful close with bounded forced process-tree fallback remains port-isolated.
- Implemented: `browser lease keepAlive`, `browser lease disable`, `--no-idle-cleanup`, and runner/config equivalents expose renewal and disable controls with effective deadlines.
- Implemented: scheduled, renewed, disabled, warning, skipped, closed, and failed states use parseable diagnostics and persist final monitor events.
- Implemented unit coverage: long renewal, concurrent ports, legacy/stale state, ownership replacement, expiry recheck, config/CLI routing, and runner lifecycle. Real-process expiry/cleanup verification is included in the implementation chunk's manual verification; broader browser-family E2E remains part of cross-browser CI coverage.

## Possible Milestones

1. Quality controls: finish presets, docs, tests, and demo GIF comparisons.
2. Choreography controls: pointer speed, easing, holds, click pulses, and caption placement.
3. Evidence reports: step-to-frame metadata, thumbnails, failure frame links, and GIF diagnostics.
4. Artifact formats: APNG/WebP/MP4 exports while preserving CMG's virtual pointer pipeline.
5. Privacy and accessibility: redaction, high-contrast pointer, focus evidence, and still-image exports.
6. Performance: streaming encoding, duplicate-frame coalescing, size budgets, and inspect/optimize commands.
