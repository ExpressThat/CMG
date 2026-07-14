# `browser control script`

Runs a `.cmgscript` browser automation script against the selected CMG-controlled browser instance. This command is the direct AI/browser-control scripting surface. Use `cmg run` when the same actions should be executed as structured tests with reports, retries, sharding, and per-test traces.

Chrome is the default. Use `--chrome` to select Chrome explicitly, `--edge` for Microsoft Edge, or `--firefox` for Firefox.

```powershell
cmg browser control script --file <path>
cmg browser control script --inline "<script>"
cmg browser --port <port> control script --file <path>
cmg browser control script --file -
cmg browser control script --file <path> --gif <path>
cmg browser control script --file <path> --gif <path> --gif-quality highest
cmg browser control script --file <path> --gif <path> --gif-dither atkinson --gif-palette local --gif-colors 192 --keep-frames <directory>
cmg browser control script --file <path> --gif <path> --gif-crop "#panel" --gif-crop-padding 24 --gif-scale 0.75 --gif-max-width 800
cmg browser control script --file <path> --gif <path> --pointer-duration 600 --pointer-easing ease-in-out
cmg browser control script --file <path> --gif <path> --pointer-theme ring --pointer-color "#dc2626" --pointer-size 44 --pointer-shadow strong
cmg browser control script --file <path> --gif <path> --show-pointer false
cmg browser control script --file <path> --gif <path> --caption-style qa --caption-position bottom --caption-severity success
cmg browser control script --file <path> --gif <path> --click-pulse ripple
cmg browser control script --file <path> --gif <path> --gif-hold-after-action 700
cmg browser control script --file <path> --gif <path> --pointer-pre-click-hold 120 --pointer-post-click-hold 450
cmg browser control script --file <path> --gif <path> --gif-hold-after-navigation 500 --gif-hold-after-assertion 650
cmg browser control script --file <path> --gif <path> --gif-hold-on-failure 1800
cmg browser control script --file <path> --gif <path> --gif-fps 20
cmg browser control script --file <path> --gif <path> --gif-frame-delay 80
cmg browser control script --file <path> --gif <path> --gif-budget 500KB
cmg browser control script --file <path> --gif <path> --gif-timeline <file-or-directory>
cmg browser control script --file <path> --trace <path>
cmg browser control script --file <path> --timeout 10000 --assertion-timeout 5000
cmg browser control script --file <path> --base-url https://example.test/app/
cmg browser control script --file <path> --var user=Ada --env mode=demo
cmg browser control script --file <path> --preview-gif-settings
cmg --chrome browser control script --file <path>
cmg --edge browser control script --file <path>
cmg --firefox browser control script --file <path>
```

## Options

- `--file <path>`: Path to a `.cmgscript` file. Specify exactly one of `--file` or `--inline`.
- `--file -`: Read script text from stdin.
- `--inline <script>`: Run inline `.cmgscript` text. Specify exactly one of `--file` or `--inline`.
- `--preview-gif-settings`: Parse the script and print effective DSL recording settings without launching, selecting, or connecting to a browser. Command-level GIF options are still validated, but preview lines describe DSL scopes.

For PowerShell automation, prefer `--file <path>` or pipe a here-string to `--file -`. PowerShell parses quotes before CMG receives `--inline`, so nested DSL, CSS, and JavaScript quotes are inherently easier to preserve through a file or stdin.
- `--gif <path>`: Optional visual-recording output path. GIF is the default; use `--record-format` for APNG, WebP, or MP4.
- `--no-gif`: Disable command-level GIF capture and every script recording block while still executing child actions. This prevents screenshots and virtual-pointer injection. `CMG_DISABLE_GIF=1` provides the same process-wide switch; enabled values are `1`, `true`, `yes`, and `on` (case-insensitive).
- `--gif-quality <archival|highest|high|medium|low>`: Recording quality for `--gif`. It controls GIF palettes, WebP lossless/lossy quality, and MP4 CRF. Defaults to `highest`.
- `--record-format <gif|apng|webp|mp4>`: Output format for command-level recording. Defaults to `gif`; a `.gif` output name is rewritten to the selected extension.
- `--record-ffmpeg <path>`: FFmpeg executable for MP4 output. Otherwise CMG uses `CMG_FFMPEG`, then `ffmpeg` on `PATH`.
- `--gif-dither <none|floyd-steinberg|bayer|atkinson|sierra>`: Override the quality preset's dithering algorithm for command-level `--gif`.
- `--gif-palette <global|local|adaptive>`: Override the GIF color table. `adaptive` currently uses frame-local tables.
- `--gif-colors <2..256>`: Override the maximum GIF palette size.
- `--gif-background <color>`: Flatten transparent capture pixels onto a named, hex, `rgb[a]`, or `hsl[a]` color.
- `--gif-gradient-mode <smooth|text>`: Prefer smooth-gradient or crisp-text defaults. Explicit encoder options still win.
- `--gif-high-contrast-palette`: Increase contrast and saturation for accessibility review. This intentionally changes source colors.
- `--gif-redact <selector>`: Solid-mask a selector in every command-level GIF frame. Repeatable.
- `--gif-mask <selector>`: Alias-style repeatable solid mask for `--gif-redact`.
- `--gif-blur <selector>`: Blur a selector in every command-level GIF frame. Repeatable and combinable with solid masks.
- `--gif-auto-redact <passwords|tokens|emails|payment|privacy|none>`: Automatic whole-run privacy masking. `privacy` combines password, token, email, and payment-card-like detection. Defaults to `passwords`; `sensitive` remains an alias for `tokens`.
- `--gif-redaction-safety <standard|strict>`: Strict mode blocks capture if sensitive content remains visibly unmasked.
- `--keep-frames <directory>`: Keep each final pre-quantization PNG as `frame-NNNN.png` in this directory. Cropping and scaling are already applied.
- `--gif-crop <selector-or-rich-locator>`: Clip each command-level GIF frame to current target bounds.
- `--gif-crop-padding <0..2000>`: Add CSS-pixel context around `--gif-crop`; requires `--gif-crop`.
- `--gif-smart-crop <true|false|widthxheight>`: Use a stable-size crop that follows the virtual pointer and active element. `true` uses `640x480`; cannot be combined with `--gif-crop`.
- `--gif-split-tabs <auto|always|none>`: Compose labeled screenshots of every tab. `always` reserves two tiles for stable popup recordings; `auto` expands when another tab exists. Cannot be combined with `--gif-crop` or `--gif-smart-crop`.
- `--gif-safe-area <0..500>`: Minimum CSS-pixel margin around pointer targets and tight crops. Defaults to `24`; use `0` to disable.
- `--gif-layout-stability <0..5000>`: Maximum target-settling window in milliseconds. CMG waits for two stable animation frames after scrolling and sticky-overlay correction. Defaults to `150`; use `0` to disable.
- `--gif-scale <0.05..1>`: Downscale the captured frame before quantization.
- `--gif-max-width <1..10000>`: Cap output width while preserving aspect ratio.
- `--gif-max-height <1..10000>`: Cap output height while preserving aspect ratio.
- `--gif-viewport <width>x<height>`: Temporarily use this CSS-pixel viewport while recording, then restore the prior viewport.
- `--gif-pixel-ratio <1..4>`: Capture at a controlled device pixel ratio for high-DPI evidence.
- `--gif-debug`: Add a frame-only diagnostics HUD and write `<gif-name>.debug.json` with one metadata record per captured frame.
- `--gif-accessibility`: Enable safe keystroke labels, amplified focus evidence, accessible role/name labels, high-contrast evidence styling, and WCAG contrast warnings for the whole GIF.
- `--gif-event-captions`: Add privacy-safe outcome captions for network, dialog, console/page-error, download, upload, service-worker, WebSocket, and worker events in the whole GIF. Payloads, URLs, expressions, results, and file paths are not copied into captions.
- `--gif-intro <text>`: Opening full-viewport title-card text for the whole GIF.
- `--gif-outro <text>`: Explicit final full-viewport title-card text. It takes precedence over a generated result outro.
- `--gif-intro-duration <milliseconds>`: Opening title-card duration. Must be greater than zero; defaults to `1200`.
- `--gif-outro-duration <milliseconds>`: Explicit or generated final title-card duration. Must be greater than zero; defaults to `1200`.
- `--gif-result-outro`: Generate a final `Test passed`, `Test failed`, or `Test skipped` card when no explicit outro is set.
- `--gif-no-coalesce`: Keep consecutive pixel-identical frames instead of merging their encoded delays. Coalescing is enabled by default.
- `--gif-sample-every <1..100>`: Keep every Nth intermediate pointer/drag movement frame. Final targets, click evidence, holds, captions, failures, and title cards are always retained.
- `--gif-budget <size>`: Target a maximum encoded size using units such as `500KB` or `2MB`. CMG retains the smallest candidate when no allowed fallback can meet the target.
- `--no-gif-budget-quality-fallback`: Preserve the requested quality while applying `--gif-budget`.
- `--no-gif-budget-downscale`: Preserve captured dimensions while applying `--gif-budget`.
- `--gif-narration <true|false|path>`: Write a UTF-8 screen-reader narration sidecar. `true` uses `<gif-name>.narration.txt`.
- `--gif-alt-text <template>`: Alt text stored in timelines/reports and used by HTML previews. Supports `{name}`, `{steps}`, `{duration}`, and `{outcome}`.
- `--gif-description <text>`: Human-written artifact description stored in timelines and JSON/HTML reports.
- `--pointer-contrast <auto|fixed>`: Adapt an uncolored pointer to the page beneath it. Defaults to `auto`.
- `--pointer-callout <auto|always|none>`: Control active-target outlines/callouts. Defaults to `auto` for tiny targets.
- `--pointer-callout-threshold <8..100>`: Tiny-target threshold in CSS pixels. Defaults to `24`.
- `--target-zoom <auto|always|none>`: Control the capture-only enlarged inset for tiny active targets. Defaults to `auto`.
- `--target-zoom-threshold <8..100>`: Auto-zoom target threshold in CSS pixels. Defaults to `24`.
- `--page-position <auto|always|none>`: Control the capture-only viewport rail for long pages. Defaults to `auto`.
- `--tab-context <auto|always|none>`: Control the capture-only active-title/tab-count badge. Defaults to `auto` for multi-tab runs.
- `--no-pointer-focus-pulse`: Disable focused-control evidence after focus-producing actions.
- `--pointer-idle <pulse|none>`: Control pointer evidence during long holds. Defaults to `pulse`.
- `--pointer-idle-threshold <100..60000>`: Long-hold threshold in milliseconds. Defaults to `1200`.
- `--no-pointer-teleport-marker`: Disable origin/path evidence for instant pointer moves.
- `--mouse-down-hold <0..60000>`: Pressed-pointer evidence hold after a real `mouseDown`. Defaults to `500ms`.
- `--pointer-duration <milliseconds>`: Default virtual pointer movement duration for command-level `--gif` recordings. Must be zero or greater.
- `--pointer-speed <slow|normal|fast|instant|multiplier>`: Default virtual pointer speed for command-level `--gif` recordings. Multipliers use the `1.5x` form. DSL block and action options can still override this.
- `--pointer-easing <linear|ease-in|ease-out|ease-in-out|spring>`: Default virtual pointer easing for command-level `--gif` recordings.
- `--pointer-path <auto|direct|arc|manhattan|avoid-target|avoid-center>`: Default virtual pointer route. `auto` is the default and uses target bounds to avoid covering large labels until the final click approach.
- `--drag-path <auto|direct|arc|manhattan|avoid-target|avoid-center>`: Default held-pointer route. When omitted it inherits `--pointer-path`.
- `--pointer-theme <arrow|hand|dot|ring|branded|touch>`: Default virtual pointer theme for command-level `--gif` recordings.
- `--pointer-color <css-color>`: Default virtual pointer color for command-level `--gif` recordings. Pass one CSS color value, not a CSS declaration.
- `--pointer-size <8..96>`: Default virtual pointer size in CSS pixels for command-level `--gif` recordings.
- `--pointer-shadow <none|light|medium|strong>`: Default virtual pointer shadow strength for command-level `--gif` recordings.
- `--show-pointer <true|false|auto>`: Default virtual pointer visibility for command-level `--gif` recordings. Defaults to `auto`, which currently shows the pointer for pointer-aware frames. Use `false` to capture frames without the DOM pointer; child actions can override with `showPointer=true`.
- `--gif-reduced-motion`: Removes default pointer travel animation and uses linear/static click evidence for the whole GIF. Explicit pointer durations still override the preset.
- `--gif-high-contrast-pointer`: Uses a large yellow ring pointer with a strong dark edge for the whole GIF. Explicit pointer theme, color, size, and shadow options override individual preset properties.
- `--caption-style <subtle|teaching|qa|bug-report|compact>`: Default caption style for command-level `--gif` recordings.
- `--caption-size <normal|large|x-large>`: Default caption text size for command-level `--gif` recordings.
- `--auto-captions`: Automatically narrate supported visual actions during command-level `--gif` recording. It is inert without an active recorder.
- `--caption-template <template>`: Whole-run automatic-caption template. Supplying it enables automatic captions. Supports `{action}`, `{selector}`, `{target}`, `{line}`, `{arguments}`, `{step}`, and `{assertion}`; unknown placeholders fail before browser connection.
- `--caption-position <top|bottom|left|right|auto>`: Default caption position for command-level `--gif` recordings.
- `--caption-severity <info|success|warning|error>`: Default caption severity color for command-level `--gif` recordings.
- `--click-pulse <ring|ripple|dot|crosshair|none>`: Default click/tap/drop pulse style for command-level `--gif` recordings. Defaults to `ring`.
- `--gif-hold-after-action <milliseconds>`: Default post-action hold for command-level `--gif` recordings. Defaults to `350`; use `0` to suppress automatic post-action holds.
- `--pointer-pre-click-hold <milliseconds>`: Default hold after pointer movement and before click/tap dispatch in command-level `--gif` recordings. Defaults to `0`.
- `--pointer-post-click-hold <milliseconds>`: Default hold after click/tap pulse frames in command-level `--gif` recordings. Defaults to `350`.
- `--gif-hold-after-navigation <milliseconds>`: Default hold after navigation actions and navigation waits in command-level `--gif` recordings. Defaults to `350`.
- `--gif-hold-after-assertion <milliseconds>`: Default hold after assertion actions in command-level `--gif` recordings. Defaults to `350`.
- `--gif-hold-on-failure <milliseconds>`: Final failure-state hold for command-level `--gif` recordings. Defaults to `1200`; use `0` to suppress the extra failure hold.
- `--gif-fps <1..100>`: Frame rate for command-level `--gif` recordings. Defaults to `10` FPS.
- `--gif-frame-delay <milliseconds>`: Frame delay for command-level `--gif` recordings. Must be `10..10000`; overrides `--gif-fps`.
- `--gif-timeline <file-or-directory>`: Optional JSON metadata sidecar for command-level `--gif`. When a directory is provided, CMG writes `<gif-name>.timeline.json` inside it.
- `--trace <path>`: Optional output path for a CMG script trace JSON file. The trace includes step names, line numbers, stdout lines, and failure reasons.
- `--timeout <milliseconds>`: Default timeout for timeout-capable waits, event waits, downloads, network waits, worker waits, tab waits, API requests, and assertions that do not set `timeout=`.
- `--navigation-timeout <milliseconds>`: Default timeout for navigation actions and navigation waits.
- `--assertion-timeout <milliseconds>`: Default timeout for assertions. Overrides `--timeout` for assertion actions.
- `--base-url <url>`: Absolute base URL used to resolve relative `navigate`, `goto`, `visit`, `openTab`, and `newContext url=` targets.
- `--var <name=value>`: Initial script variable. Can be repeated. Later entries with the same name replace earlier entries.
- `--env <name=value>`: Alias for `--var`, intended for CI and agent-provided environment values.

## Behavior

- Requires a browser started with [`browser launch`](../launch.md), except when `--preview-gif-settings` is used. For Edge, use `cmg --edge browser launch`. For Firefox, use `cmg --firefox browser launch`.
- Use `cmg browser --port <port> control script --file <path>` when the target browser was launched with `cmg browser --port <port> launch`.
- Use [`validateScript`](validateScript.md) to check imports and syntax before connecting to a browser.
- `--file -` fails with `No script text was provided on stdin for --file -.` when stdin is empty.
- Executes actions in file order and stops on the first failed action.
- `skip "reason"` stops the script as skipped, writes `SKIP <line> <reason>` to stdout, and exits `0`.
- Writes step logs and action outputs to stdout.
- Writes validation, parse, browser, and action errors to stderr.
- CDP HTTP, WebSocket, cancellation, and timeout failures are converted into a browser connection error on stderr and exit code `1`; a cold attach cannot fail without a diagnostic result.
- Supports line-level `import "path"` statements. Relative imports resolve from the script file's directory.
- Supports the shared CMG action surface documented in the [action index](../../../scripting/action-index.md) and [action reference](../../../scripting/actions.md).
- Supports control flow, scoped variables, `set` block capture, macros, loops, `try`/`catch`/`finally`, `within`, frame blocks, `step`, `recording` / `withRecording` / `recordingDefaults` scoped defaults, mutable `setRecording` defaults, and `gif` blocks.
- Relative navigation targets are resolved against `--base-url` before the browser is asked to navigate.
- Initial `--var` and `--env` values are available as `${name}` before the first action, macro call, condition, or `set` block runs.
- `set` is a script action for scoped variables and command-result capture. It is intentionally not a CLI command because it only has meaning inside a script scope.
- Uses the selected browser automation protocol through the active CMG endpoint: Chrome DevTools Protocol for Chrome and Edge, WebDriver BiDi for Firefox.
- Browser diagnostics are armed automatically when CMG launches or attaches to a browser/app. Console messages and page errors accumulate in page-side buffers between CMG commands from that arming point forward. Use `listConsole`, `listPageErrors`, `expectNoConsole`, and `expectNoPageError` after risky interactions. Events that occurred before launch/attach/diagnostics arming cannot be recovered.
- Browser JavaScript dialogs are handled explicitly. CMG does not silently remove, accept, or dismiss dialogs through the browser protocol. Add `captureDialogs` or `setDialogBehavior` before the action that opens an `alert`, `confirm`, or `prompt`.

## GIF Behavior

- When `--gif` is provided, captures the visible page viewport after visual actions and writes an animated GIF.
- GIF quality defaults to `highest`, which uses CMG's most color-faithful palette matching and dithering. Use `high`, `medium`, or `low` only when smaller/faster GIF artifacts matter more than color fidelity.
- The `set` variable action is logged but does not add a standalone frame because it has no page-visible effect.
- Script-level `gif "name" { ... }`, `recordVideo "name" { ... }`, and `screencast "name" { ... }` blocks record only the wrapped actions when `--gif` is not provided.
- With `--no-gif` or enabled `CMG_DISABLE_GIF`, stdout emits `GIF_DISABLED source=<cli|environment>` and each suppressed block emits `GIF_SKIPPED <line> status=skipped reason=recording-disabled source=<cli|environment>`. Child actions still run; no GIF, retained frame, screenshot, recording overlay, or virtual pointer is created.
- `gifIfChanged` / `gif.ifChanged` writes only when final page pixels differ from its baseline; `gifOnFailure` / `gif.onFailure` writes only for a failed block. `gifSnapshot` / `gif.snapshot` adds a named hold to an active recorder.
- When `--gif` is provided, the whole script is recorded and nested block recordings are suppressed.
- GIF recording adds a virtual pointer in the browser page. The pointer is visible live during recording and is captured in the GIF frames.
- Without `--gif` or an active script-level recording block, CMG does not inject the virtual pointer. Recording-only actions such as `pauseGif`, `moveMouse`, `recordCheckpoint`, `gifSnapshot`, `showPointer`, `hidePointer`, `pointerStyle`, `annotateTarget`, `highlightTarget`, and `recordVariable` are skipped instead of reading target/variable state or creating pointer frames and timeline entries.
- Whole-run encoder flags are inert without `--gif`; `--keep-frames` does not capture screenshots or inject a virtual pointer by itself.
- Pointer-aware actions resolve rich locators to the same target used by browser dispatch, so pointer movement, pointer events, hover state, drag ghosts, screenshots, and captions stay aligned.
- DSL recording scopes and blocks can set `autoCaptions=true` and `captionTemplate=`. Automatic captions use privacy-safe text-entry defaults and target-aware `captionPosition=auto`; without an active GIF they do not modify the page.
- Active GIF recordings automatically caption successful assertions with expected/actual QA evidence and failed actions with bounded bug-report evidence. Sensitive assertion values are masked. DSL `assertionCaptions=false` and `failureCaptions=false` opt out.
- `caption` and nestable `narrate` blocks support `captionDuration=` / `duration=`, `fadeIn=`, and `fadeOut=` deterministic encoded timing.
- DSL recording scopes and blocks can set `intro=`, `outro=`, `introDuration=`, and `outroDuration=`. Explicit `intro` and `outro` actions capture chapter cards. All title-card forms are recording-only and never create a pointer or overlay in non-GIF runs.
- `hideFromGif` / `cutGif` execute child actions without recording frames or pointer UI. `speedUpGif factor=` and `slowDownGif factor=` scale encoded delays locally. These blocks are nestable and execute normally with no pointer when GIF recording is inactive.
- Whole-run pointer, caption, accessibility, and timing defaults from `--pointer-duration`, `--pointer-speed`, `--pointer-easing`, `--pointer-theme`, `--pointer-color`, `--pointer-size`, `--pointer-shadow`, `--show-pointer`, `--caption-style`, `--caption-position`, `--caption-severity`, `--caption-size`, `--auto-captions`, `--caption-template`, `--gif-accessibility`, `--pointer-pre-click-hold`, `--pointer-post-click-hold`, `--gif-hold-after-action`, `--gif-hold-after-navigation`, `--gif-hold-after-assertion`, `--gif-hold-on-failure`, `--gif-fps`, and `--gif-frame-delay` apply when `--gif` is active. DSL `recording` / `withRecording`, `gif`, `recordVideo`, and `screencast` blocks can set matching scoped defaults for child actions; child actions can override action options locally.
- If the script fails, CMG still writes a partial GIF containing frames captured before the failure.
- On failure, command-level GIF recording captures one extra final-state hold frame before writing the partial GIF unless `--gif-hold-on-failure 0` is used.
- `--gif-timeline` writes a JSON sidecar after the GIF is saved and emits `GIF_TIMELINE <path>` on stdout. The sidecar includes the GIF path, file size, dimensions, frame count, frame delays, total duration, quality, encoder controls, framing controls, and recorder timing settings.
- Whole-run redaction defaults apply before every screenshot and are removed immediately afterward. Timeline `redactions` records configured rules and audit events without recording secret values. Redaction options are inert without `--gif` and never inject a virtual pointer by themselves.
- `--gif-debug` emits `GIF_DEBUG <path>` after writing the debug sidecar. Each frame record includes timing, action, source line, nested scope, target selector, and virtual-pointer coordinates.
- `--gif-budget <size>` targets an encoded size using quality fallback and then bounded downscaling; `--no-gif-budget-quality-fallback` and `--no-gif-budget-downscale` disable either fallback. The smallest candidate is retained when the target is impossible.
- Every completed recording emits `GIF_CAPTURE_STATS` with frame, optimization, blank-frame, ICC/CICP/gamma, profile-change, memory, timing, and budget-decision counts. `GIF_WARN_UNCHANGED`, `GIF_WARN_BLANK`, and `GIF_WARN_COLOR_PROFILE` explain evidence-quality risks without changing the exit code.
- Every retained artifact emits `GIF_REPRODUCE path="..." command="..."`. The JSON-escaped command preserves browser selection, `browser --port` placement, and the file/inline source. It includes `--gif` for command-level recording and omits it for focused DSL blocks so the block remains the recording boundary.
- `narrationSidecar=`, `altText=`, and `description=` work on `gif`, `recordVideo`, `screencast`, and inherited recording scopes. A completed sidecar emits `GIF_NARRATION <absolute-path>`; no sidecar is created without an active recorder.
- APNG and WebP use built-in encoders. MP4 uses FFmpeg with H.264, `yuv420p`, even-dimension padding, and an exact 100 fps centisecond evidence timeline. A missing executable fails with an actionable stderr reason and exit code `1`; CMG never writes GIF bytes under another extension.
- Active recordings also emit `GIF_WARN_MULTIPLE_TARGETS`, `GIF_WARN_TINY_TARGET`, `GIF_WARN_SCROLLED`, and `GIF_WARN_NON_VISUAL` when selector or action choices weaken the resulting evidence. These warnings never fail the script and are not evaluated when GIF recording is inactive.
- Suppressed nested recording blocks emit `GIF_BLOCK_SUPPRESSED <line> reason=command-level-recording` when `--gif` owns the whole run.

## Trace Behavior

When `--trace` is provided, CMG writes a whole-run trace JSON file even when the script fails after tracing has started. Nested `startTracing` / `stopTracing` actions are suppressed during command-level tracing and emit `TRACE_BLOCK_SUPPRESSED`.

## Stdout

Successful actions write parseable lines:

```text
PASS 001 line=1 action=navigate C:\Projects\CMG\index.html
NAVIGATED 001 file:///C:/Projects/CMG/index.html
PASS 002 line=2 action=waitForElement #openProfileDialog
PASS 003 line=3 action=step Open dialog
PASS 004 line=4 context="step Open dialog" action=click #openProfileDialog
PASS 005 line=5 action=setDefaultTimeout 10000
DEFAULT_TIMEOUT 005 10000
PASS 006 line=6 action=screenshot #profileDialog
SCREENSHOT 006 C:\Projects\CMG\profile-dialog.png
PASS 007 line=7 action=evaluate document.title
EVALUATE 007 CMG Browser Control Test Page
GIF C:\Projects\CMG\demo-output\dialog-flow.gif
GIF_FRAMES path="C:\\Projects\\CMG\\demo-output\\dialog-flow.frames" count=14
GIF_TIMELINE C:\Projects\CMG\demo-output\dialog-flow.timeline.json
GIF_DEBUG C:\Projects\CMG\demo-output\dialog-flow.debug.json
GIF_PAUSE 008 milliseconds=800 status=captured
GIF_FAILURE_CAPTION 009 action="expectText" status=captured
GIF_WARN_MULTIPLE_TARGETS line=10 action=click selector=".save" count=2
GIF_WARN_TINY_TARGET line=10 action=click selector=".save" width=12 height=12 threshold=16
GIF_WARN_SCROLLED line=11 action=click selector="#finish" reason=offscreen-target
GIF_WARN_NON_VISUAL line=12 action=recordCheckpoint options=pointerDuration
TRACE C:\Projects\CMG\demo-output\dialog-flow.trace.json
SKIP 007 Feature flag disabled
```

`PASS` sequence numbers increase globally for the whole script and include `line=<line> action=<action>`. Nested output from a macro, loop, `step`, retry, or handled branch also includes `context="..."`. Payload lines stay compact at top level and use the same sequence as their action when nested context metadata is present. Runner JSON/HTML reports expose sequence, source line, context, and action as structured fields for every step.

Action-specific payload lines are documented in the [action reference](../../../scripting/actions.md).

Preflight writes one line for each recording scope or artifact and any actionable warning:

```text
GIF_SETTINGS scope=recordingdefaults line=2 action=recordingDefaults options=captionStyle=qa,quality=highest
GIF_SETTINGS scope=gif line=3 action=gif options=captionStyle=qa,pointerSpeed=slow,quality=highest
GIF_SETTINGS_WARN line=8 action=evaluate option=pointerDuration reason=non-visual-action
GIF_SETTINGS_WARN line=3 action=recordVideo reason=gif-alias format=gif suggestion=use-gif
GIF_SETTINGS_WARN line=3 action=recordVideo reason=long-recording-block actions=21 threshold=20 suggestion=split-or-cut
```

`reason=gif-alias` explains that `recordVideo` and `screencast` still produce animated GIFs. `reason=long-recording-block` counts all descendant actions and suggests splitting the focused evidence or using `hideFromGif` / `cutGif` around irrelevant work. These advisory stdout lines do not require a browser and do not change the exit code.

During execution, `setRecording` writes `RECORDING_SETTINGS <line> options=<effective-settings>` and `previewRecordingSettings` writes `GIF_SETTINGS <line> options=<effective-settings>`.

`GIF_FRAMES path="<JSON-escaped-absolute-directory>" count=<frames>` is emitted only when source-frame retention is active. The retained PNGs contain the same visible page and CMG recording UI as their corresponding GIF frames and should be handled as sensitive artifacts.

`GIF_REPRODUCE path="<JSON-escaped-absolute-gif>" command="<JSON-escaped-command>"` is emitted once per retained GIF. The command is a deterministic minimal reproduction route; script-owned recording options remain in the source file.

`GIF_NARRATION <absolute-path>` is emitted after a requested screen-reader narration file is written. The UTF-8 file contains the authored description, rendered alt text, duration, outcome, and ordered runtime steps without page values or entered secrets.

## Stderr

`--preview-gif-settings` writes parse, import, and invalid recording-option errors to stderr. It produces no browser connection errors because it never connects. Authoring warnings remain on stdout and do not make a valid preview fail.

Failure output includes the script line number, action, and reason:

```text
Line 4: waitForElement failed. No element matched selector '#missing'.
Line 8: click failed in macro login > repeat[2/3]. No element matched selector '#save'.
```

Invalid encoder values fail before browser connection and name the option, including invalid `background=` colors and `gradientMode=` values.

Invalid framing values use the same pre-browser failure path and name `cropPadding=`, `safeArea=`, `layoutStability=`, `scale=`, `maxWidth=`, or `maxHeight=`. A missing crop target during recording fails the responsible action with the selector resolution reason. Framing controls do not evaluate page scripts, inject a pointer, or capture frames when GIF recording is inactive.

## Exit Codes

With `--preview-gif-settings`, exit code `1` means invalid input, import, syntax, command-level GIF option, or DSL recording option. A browser is not required.

- `0`: Script completed successfully.
- `0`: Script stopped with `skip "reason"`.
- `1`: Browser is not running, script cannot be read, script syntax is invalid, or an action fails.
- `1`: A GIF encoder option is invalid or `--gif-colors` is outside `2..256`.

## Example

```text
navigate "C:\Projects\CMG\index.html"
waitForElement "#openProfileDialog" timeout=5000
setDefaultAssertionTimeout 5000

step "Open the profile dialog" {
  click "#openProfileDialog"
  waitForElement "#profileDialog[open]"
}

type "#profileName" "CMG Test Profile"
screenshot "#profileDialog" output="profile-dialog.png"
assertText "#lastDialogAction" "None"
```

Run a script with initial variables:

```powershell
cmg browser control script --file demo-scripts\139-cli-variables.cmgscript --var user=Ada
cmg browser control script --file demo-scripts\141-base-url.cmgscript --base-url https://example.test/app/
cmg browser control script --file demo-scripts\148-gif-quality.cmgscript --gif demo-output\quality.gif --gif-quality highest
cmg browser control script --file demo-scripts\149-gif-pointer-choreography.cmgscript --gif demo-output\pointer-choreography.gif --pointer-duration 500 --pointer-pre-click-hold 120 --pointer-post-click-hold 450
cmg browser control script --file demo-scripts\227-gif-auto-pointer-path.cmgscript --gif demo-output\auto-path.gif --pointer-path auto --drag-path avoid-target
cmg browser control script --file demo-scripts\180-gif-accessible-presets.cmgscript --gif demo-output\accessible-presets.gif --gif-reduced-motion --gif-high-contrast-pointer
cmg browser control script --file demo-scripts\184-gif-contrast-captions.cmgscript --gif demo-output\accessible-review.gif --gif-accessibility --caption-size x-large
cmg browser control script --file demo-scripts\186-gif-event-captions.cmgscript
cmg browser control script --file demo-scripts\188-gif-result-cards.cmgscript
cmg browser control script --file demo-scripts\190-gif-capture-efficiency.cmgscript
cmg browser control script --file demo-scripts\156-gif-pointer-styles.cmgscript --gif demo-output\pointer-styles-whole-run.gif --pointer-theme branded --pointer-color "#2563eb"
cmg browser control script --file demo-scripts\157-gif-caption-styles.cmgscript --gif demo-output\caption-styles-whole-run.gif --caption-style bug-report --caption-position bottom
cmg browser control script --file demo-scripts\150-gif-failure-hold.cmgscript --gif demo-output\failure-hold.gif --gif-hold-on-failure 1800 --gif-timeline demo-output\timelines
cmg browser control script --inline "listConsole level=error"
```

More syntax and action details are documented in the [scripting guide](../../../scripting/index.md). Style guidance is in the [CMG script style guide](../../../scripting/style-guide.md).
