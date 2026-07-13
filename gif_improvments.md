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
- Implemented for DSL recording blocks: sidecar PNG frame export preserves final pre-quantization frames; without crop/scale transforms these remain the exact browser PNG bytes.
- Implemented: DSL `keepFrames=<directory|true|false>` and whole-run CLI `--keep-frames <directory>`. Runner output is isolated per GIF for parallel safety.
- Implemented: `cmg gif color-diff <source.png> <encoded.gif> --frame <n>` reports parseable MAE, RMS, maximum-channel, and changed-pixel encoder drift.
- Implemented for gradients and exact source-byte retention: automated color-delta tests. Brand-color, shadow, and transparency coverage remains open.
- Implemented: direct and structured-runner gradient/brand-color fidelity fixtures in demos 165 and 166. Broader browser visual baselines remain open.
- Add `background=<color>` for pages with transparent captures. CLI: `--gif-background <color>`.
- Add color-space metadata handling checks for browser screenshots.
- Add a high-contrast palette mode for accessibility review GIFs.
- Add `dither=none` for sharp UI screenshots where dithering looks noisy.
- Add `gradientMode=smooth` that prefers gradient fidelity over crisp text edges. CLI: `--gif-gradient-mode <smooth|text>`.

## Virtual Pointer Choreography

- Add `pointerSpeed=<slow|normal|fast|instant>` on `recording`, `gif`, `recordVideo`, and `screencast` blocks.
- Add action-level `pointerSpeed=` for pointer-aware actions such as `click`, `hover`, `tap`, `dragAndDrop`, `moveMouse`, `wheel`, `select`, `fill`, and `uploadFiles`.
- Add child-level `pointerSpeed=` inside block `dragAndDrop { ... }` for `moveMouse`, `hover`, `delay`, and `drop` choreography.
- Add CLI `--pointer-speed <slow|normal|fast|instant|multiplier>` only as a whole-run default when command-level recording is active.
- Add numeric pointer speed such as `pointerSpeed=1.5x`.
- Add `pointerDuration=<ms>` to control movement duration between targets. It should work on `recording`, `gif`, `recordVideo`, `screencast`, and individual pointer-aware actions. CLI: `--pointer-duration <ms>`.
- Add `pointerDuration=<ms>` on `dragAndDrop` to control drag travel duration specifically, without changing earlier pointer movement to the source.
- Add `sourcePointerDuration=<ms>` and `targetPointerDuration=<ms>` on `dragAndDrop` for flows where moving to the source should be quick but the drag travel should be slow and readable.
- Add `dropPointerDuration=<ms>` for block `dragAndDrop { drop "#target" }` to control the final drop move.
- Add `pointerEasing=<linear|ease-in|ease-out|ease-in-out|spring>` for movement style at recording scope, block scope, and action scope. CLI: `--pointer-easing <mode>`.
- Add `dragEasing=<mode>` on `dragAndDrop` for drag travel independent of normal pointer travel.
- Add per-action pointer speed overrides, for example `click "#save" pointerSpeed=fast`.
- Add `moveMouse duration=<ms>` and `moveMouse easing=<mode>`.
- Add `moveMouse pointerDuration=<ms>` as an alias for `duration=<ms>` so pointer settings stay consistent across actions.
- Implemented: add pointer path styles: direct line, curved arc, Manhattan path, avoid-target, and avoid-center.
- Implemented: add `pointerPath=<direct|arc|manhattan|avoid-target|avoid-center>` on recording blocks and pointer-aware actions.
- Implemented: add `dragPath=<direct|arc|manhattan|avoid-target|avoid-center>` on `dragAndDrop`.
- Add auto pathing that avoids covering the target text or button label.
- Implemented: add `preClickHold=<ms>` and `postClickHold=<ms>` at recording-block and action level. CLI: `--pointer-pre-click-hold <ms>` and `--pointer-post-click-hold <ms>`.
- Implemented: add `preDragHold=<ms>`, `dragHold=<ms>`, and `postDropHold=<ms>` on `dragAndDrop`.
- Implemented: add `holdAfterMove=<ms>` on `moveMouse` for demonstrations where the pointer should settle before the next action.
- Add click pulse style options: ring, ripple, dot, crosshair, or none. CLI: `--click-pulse <style>`. Default should be `ring` so clicks are visible evidence unless a script disables it with `clickPulse=none`.
- Implemented: add right-click and middle-click distinct visual pulses.
- Implemented: add double-click pulse choreography that clearly shows two clicks.
- Implemented: add drag trail rendering for long drags.
- Implemented: add optional drag path breadcrumb dots.
- Add a visible hold state for `mouseDown`.
- Implemented: add a pressed pointer state while dragging.
- Implemented: add touch pointer styling for `tap` and `touchTap`.
- Add keyboard focus pulse when actions do not move the pointer.
- Add pointer target callout lines for tiny elements.
- Add automatic pointer zoom/callout for elements smaller than a threshold.
- Implemented: add a configurable pointer theme: system arrow, hand, dot, ring, touch, or branded pointer. CLI: `--pointer-theme <theme>`.
- Implemented: add pointer color and size options. CLI: `--pointer-color <color>` and `--pointer-size <pixels>`.
- Implemented: add pointer shadow strength options for dark/light pages. CLI: `--pointer-shadow <none|light|medium|strong>`.
- Add automatic pointer contrast against the current page background.
- Implemented: add `showPointer=<true|false|auto>` for recording scopes, recording blocks, and pointer-aware action overrides. CLI: `--show-pointer <true|false|auto>`.
- Add pointer idle animations for long waits so the GIF does not look frozen.
- Add pointer teleport markers for `instant` speed so jumps remain understandable.

## Captions And Narration

- Implemented: add caption style presets: subtle, teaching, QA evidence, bug report, compact.
- Implemented: add `captionStyle=<subtle|teaching|qa|bug-report|compact>` on recording blocks, `step`, and captions. CLI: `--caption-style <style>`.
- Implemented: add `captionPosition=<top|bottom|left|right|auto>` on recording blocks, `step`, and captions. CLI: `--caption-position <position>`.
- Implemented for automatic action captions: `captionPosition=auto` places the caption opposite the active target's viewport half. Target-aware placement for standalone manual captions remains open.
- Implemented in DSL recording scopes and recording blocks: `autoCaptions=true`. A whole-run CLI default remains open.
- Implemented in DSL recording scopes, recording blocks, and action overrides: `captionTemplate=` with `{action}`, `{selector}`, `{target}`, `{line}`, and `{arguments}`. A whole-run CLI default and structural `{step}` / `{assertion}` context placeholders remain open.
- Implemented: `caption duration=<ms>` / `captionDuration=<ms>` controls encoded visibility duration and can be inherited from recording scopes.
- Implemented: `fadeIn=<ms>` and `fadeOut=<ms>` capture deterministic opacity stages and remove completed captions.
- Add caption stacking for nested `step` blocks.
- Add a persistent step title bar option.
- Add optional source-line captions for debugging.
- Implemented: automatic bounded failure captions with `failureCaptions=false` opt-out and parseable `GIF_FAILURE_CAPTION` diagnostics.
- Implemented: automatic assertion captions show expected vs actual values, mask sensitive values, and support `assertionCaptions=false` opt-out.
- Add network captions for request waits and mocks.
- Add dialog captions for alert/confirm/prompt handling.
- Add console/page-error captions when captured events occur.
- Implemented: add caption severity styles: info, success, warning, error. CLI: `--caption-severity <severity>` for whole-run defaults.
- Add caption localization hooks.
- Add markdown-like caption formatting for bold/code snippets.
- Implemented: nestable `narrate "message" { ... }` teaching-style blocks with scoped execution context and timing options.
- Add automatic captions for `try/catch/finally` transitions.
- Add automatic captions for loop iterations and macro calls when debug narration is enabled.

## Timeline And Editing Controls

- Implemented: add `fps=<number>` on recording scopes for output frame rate. CLI: `--gif-fps <number>`.
- Implemented: add `frameDelay=<ms>` for advanced timing. CLI: `--gif-frame-delay <ms>`. Explicit `frameDelay` overrides `fps`.
- Implemented: `holdAfterAction=<ms>` as a block/action default and CLI `--gif-hold-after-action <ms>` for whole-run recordings.
- Implemented: `pauseGif <ms>` as a recording-only hold action that captures a timed GIF frame when recording is active and skips without injecting the virtual pointer when no GIF recorder is active.
- Implemented: add `holdAfterNavigation=<ms>`. CLI: `--gif-hold-after-navigation <ms>`.
- Implemented: add `holdAfterAssertion=<ms>`. CLI: `--gif-hold-after-assertion <ms>`.
- Implemented: `holdOnFailure=<ms>` so final failure state is readable. CLI: `--gif-hold-on-failure <ms>`.
- Implemented in DSL: add `intro "<text>"` and `recording intro="..." introDuration=<ms> { ... }` title cards. A whole-run `--gif-intro` CLI default remains open.
- Implemented in DSL: add `outro "<text>"` and `recording outro="..." outroDuration=<ms> { ... }` final summary cards. A whole-run `--gif-outro` CLI default remains open.
- Add automatic pass/fail outro cards for test runs.
- Add `pauseGif <ms>` action for recording-only holds.
- Implemented: add `cutGif { ... }` to execute boring waits/setup while suppressing their frames, pointer, captions, checkpoints, and nested GIF artifacts.
- Implemented: add nestable `speedUpGif factor=<number> { ... }` blocks for long setup sections.
- Implemented: add nestable `slowDownGif factor=<number> { ... }` blocks for important interactions.
- Implemented: add `hideFromGif { ... }` as the intent-revealing alias for cut sections.
- Implemented: `recordCheckpoint "name"` markers emit parseable step output and write frame/time bookmarks into GIF timeline JSON when recording is active.
- Implemented: add timeline metadata JSON beside each GIF. CLI: `--gif-timeline <file|directory>`; DSL blocks support `timeline=<true|false|file|directory>`.
- Add an editor command to trim start/end frames after recording.
- Implemented: add `cmg gif optimize <file> --output <gif>` to remove consecutive duplicate frames while preserving duration.
- Add automatic duplicate-frame coalescing.
- Add automatic long-wait compression while preserving a visible timer.
- Add visual progress bar for long waits.
- Add frame bookmarks for steps and failures.
- Implemented: add `cmg gif storyboard <file> --output <png>` to export GIF frames as a contact sheet PNG.
- Add timeline preview in HTML reports.

## Viewport, Layout, And Framing

- Add `viewport=<width>x<height>` recording setting independent of browser viewport setup. CLI: `--gif-viewport <width>x<height>`.
- Implemented: `scale=<0.05..1>` downscales complete composited frames. CLI: `--gif-scale <number>`.
- Implemented: `maxWidth=` and `maxHeight=` preserve aspect ratio. CLI: `--gif-max-width <pixels>` and `--gif-max-height <pixels>`.
- Add high-DPI capture support with controlled CSS pixel to output pixel scaling.
- Implemented: `crop="<selector-or-rich-locator>"` resolves live for every frame. CLI: `--gif-crop <selector>`.
- Implemented: `gif "name" crop="#panel" { ... }` block support, with `cropPadding=` / `--gif-crop-padding` for context.
- Add smart crop that follows the pointer and active element.
- Add split-screen recording for multi-tab or popup flows.
- Add frame recording inside iframes with visible page context.
- Add automatic scroll-to-keep-target-visible behavior.
- Add safe-area padding so pointer/captions are not clipped.
- Add option to blur or mask page regions during GIF recording. CLI: repeatable `--gif-mask <selector>` and `--gif-blur <selector>` for coarse defaults.
- Add `maskGif "<selector>"` and `unmaskGif "<selector>"` actions.
- Add redaction presets for emails, tokens, passwords, and credit-card-like text.
- Add live element highlight outlines that follow the target through the action.
- Add target zoom inset, like a magnifying glass, for tiny controls.
- Add mini-map or viewport position indicator for long pages.
- Add automatic viewport stabilization after layout shifts.
- Add handling for sticky headers that would cover scrolled targets.

## Script And DSL Controls

- Add recording options to `gif`, `recordVideo`, and `screencast`: `quality=`, `fps=`, `pointerSpeed=`, `captionStyle=`, `crop=`, `scale=`.
- Implemented: add `recording { ... }` block for shared recording settings and make it inherit through nested macros, loops, `try/catch`, `within`, frame scopes, nested recording blocks, and command-level `--gif` action choreography.
- Add `setRecording quality=highest pointerSpeed=normal` action.
- Implemented: add scoped recording settings that restore after the block.
- Implemented: add `withRecording quality=highest pointerDuration=500 { ... }` as a readable alias if `recording { ... }` feels too much like it must record by itself.
- Add `recordingDefaults { ... }` or config-level defaults for teams that want every script in a folder to share a house style.
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
- Add `showKeystrokes` block for keyboard-heavy flows.
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
- Add failure frame thumbnail in HTML reports.
- Add step-to-frame mapping in JSON reports.
- Add report links that jump to the frame where a step started.
- Add report links that jump to the failure frame.
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
- Add browser dialog overlays that show accepted/dismissed/prompted state.
- Add download event overlays and artifact links.
- Add file upload overlays that show selected filenames safely.
- Add support for browser zoom level detection and correction.
- Add support for prefers-reduced-motion testing while still making GIFs understandable.
- Add support for pages using CSS transforms, zoom, and nested scrolling containers.
- Add conservative automatic cleanup for CMG-launched headless browsers when an agent forgets to close them. Track CMG ownership by browser kind, port, PID, and launch token; renew an activity heartbeat whenever that instance is used; use a long configurable idle grace period so an agent can do other work and return later; warn before lease expiry where a persistent agent can observe it; close immediately only when a runner explicitly owns a short-lived lifecycle; reclaim genuinely orphaned instances after the renewable lease expires; never close user-launched, manually attached, recently active, or explicitly keep-alive browsers; emit parseable cleanup diagnostics; and allow CLI/config/environment overrides for lease duration, cleanup disablement, and keep-alive behavior.

## Performance And Storage

- Add streaming GIF encoding to avoid holding every frame in memory.
- Add frame diffing to store only changed regions where the encoder supports it.
- Add duplicate-frame detection before encoding.
- Add configurable frame sampling for long recordings.
- Add parallel frame preprocessing before final encode.
- Add memory usage diagnostics for long GIFs.
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

- Add `debug=true` on recording blocks to write metadata about every captured frame. CLI: `--gif-debug`.
- Add overlay showing current action name and source line.
- Add overlay showing current macro/loop/step context.
- Add overlay showing current selector and resolved target rectangle.
- Add overlay showing mouse coordinates.
- Add overlay showing scroll position.
- Add overlay showing active browser tab/context.
- Add warning when an action was non-visual and did not capture a meaningful frame.
- Add warning when pointer target is offscreen and had to be scrolled.
- Add warning when a selector resolved to multiple elements.
- Add warning when a target is too small to see clearly.
- Add warning when a capture is mostly blank or all white/black.
- Add warning when repeated frames indicate the page did not visually change.
- Add diagnostics when the pointer could not be promoted above page UI.
- Add diagnostics when GIF blocks are suppressed by command-level recording.
- Add diagnostics for invalid recording settings in runner reports.
- Add a generated reproduction command for every GIF.

## Accessibility And Review Use Cases

- Add keyboard overlay for keyboard-only flows.
- Add focus ring amplification for accessibility evidence.
- Add ARIA role/name captions for clicked controls.
- Add color-contrast warning overlay option.
- Add reduced-motion recording preset.
- Add high-contrast pointer preset.
- Add larger caption text preset.
- Add screen-reader narration sidecar text file.
- Add WCAG evidence mode that captures focus order and accessible names.
- Add alt text generation template for GIF artifacts.
- Add report field for human-written GIF description.
- Add option to export a step-by-step still image PDF for reviewers who cannot view GIFs.

## Privacy And Security

- Add `redactText "<selector>"` action for GIF-only redaction.
- Add automatic password input masking in recordings.
- Add automatic token-like text masking.
- Add `redact="<selector>"` and repeatable DSL redaction actions. CLI: repeatable `--gif-redact <selector>` only as a coarse whole-run safety default.
- Add per-project redaction config.
- Add redaction audit metadata in reports.
- Add fail-safe mode that refuses to record when unredacted password fields are visible.
- Add blur, solid mask, and replacement text redaction styles.
- Add option to hide URL bar or browser chrome if future capture includes chrome surfaces.
- Add environment-variable controlled recording disable switch for sensitive CI. CLI/env: `CMG_DISABLE_GIF=1` and `--no-gif`.

## Authoring Experience

- Add `previewRecordingSettings` validation/action support to print effective DSL recording settings.
- Add `cmg browser control script --preview-gif-settings` only as a convenience wrapper over the same analysis.
- Implemented: add `cmg gif presets` to list quality, pointer speed, pointer easing, click pulse, and timing presets.
- Add docs with side-by-side examples of quality presets.
- Add generated sample GIFs to docs for visual comparison.
- Add a quick-start command specifically for first GIF creation.
- Add inline suggestions when users use `recordVideo` expecting MP4.
- Add warnings that `recordVideo` and `screencast` are GIF aliases unless `format=` or a future whole-run format default changes that.
- Add VS Code snippets for GIF blocks and recording settings.
- Add script validation warnings for ignored recording options.
- Add lint rules for overly long GIF blocks.
- Add lint rule suggesting `gif=onFailure` or the equivalent whole-run CLI default for large test suites.
- Add script style-guide section for readable visual evidence.

## Agent Browser Lifecycle Safety

- Add opt-in cleanup for headless browsers abandoned by an agent. This must not eagerly close a browser merely because no command is currently running.
- Use a long, configurable idle lease measured in minutes, with cleanup disabled by default until a caller or project enables it. CLI candidates: `browser launch --idle-timeout <duration>` and `run --browser-idle-timeout <duration>`.
- Refresh the lease after every command sent to that browser and while a script or test run is active. Normal agent reasoning, file edits, report inspection, and other work between browser commands must fit comfortably inside the default enabled timeout.
- Track CMG ownership, browser kind, port, process id, launch time, and last activity. Never terminate an attached browser or a process CMG did not launch unless the caller explicitly opts into that behavior.
- Prefer graceful browser shutdown, then use bounded forced cleanup only when the owned process does not exit. Clean up child processes and temporary profiles without affecting another CMG browser on a different port.
- Add a lease-renewal/keepalive command for long non-browser work and an explicit `--no-idle-cleanup` override. Agent-facing output must state the effective deadline and how to extend or disable it.
- Emit structured diagnostics when cleanup is scheduled, renewed, skipped, or performed, and include the reason, browser port, process id, ownership status, and idle duration.
- Cover active work, long pauses, concurrent ports, attached browsers, crashed agents, stale state files, process-id reuse, graceful close, forced fallback, and cleanup races with unit and end-to-end tests.

## Possible Milestones

1. Quality controls: finish presets, docs, tests, and demo GIF comparisons.
2. Choreography controls: pointer speed, easing, holds, click pulses, and caption placement.
3. Evidence reports: step-to-frame metadata, thumbnails, failure frame links, and GIF diagnostics.
4. Artifact formats: APNG/WebP/MP4 exports while preserving CMG's virtual pointer pipeline.
5. Privacy and accessibility: redaction, high-contrast pointer, focus evidence, and still-image exports.
6. Performance: streaming encoding, duplicate-frame coalescing, size budgets, and inspect/optimize commands.
