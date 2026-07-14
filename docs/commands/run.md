# `run`

Runs CMG DSL test files.

```powershell
cmg run <path> [options]
```

`<path>` can be one `.cmgscript` file or a directory. Directories are searched recursively for `.cmgscript` files.

Use `--config <file>` to load repeatable runner defaults from a JSON file. CLI options override config values, and `--var` / `--env` override values from the config `variables` object. Relative artifact paths inside the config resolve from the config file's directory. Config files can define named `projects` for cross-browser CI matrices; select one with `--project <name>`.

`cmg run` executes structured `.cmgscript` tests. Top-level browser actions must be wrapped in `test`/`it`/`specify` or `suite`/`describe`/`context` blocks. Direct browser-control scripts run with [`browser control script`](browser/control/script.md). See the [migration guide](../scripting/migration.md) when moving a direct script into the runner.

The runner supports line-level `import "path"` statements. Relative imports resolve from the importing file's directory before parsing. Top-level macros from the file or imported files are registered before each test, and suite-level macros are registered before tests in that suite.

Runner hooks include `beforeAll`, `afterAll`, `beforeEach`, and `afterEach`. Once hooks run for the first or last non-skipped selected test in their file or suite scope, so grep/tag/only/shard selection controls which scopes execute setup and teardown.

Parameterized runner tests use `test.each`, `it.each`, or `specify.each` with `values=`, `each=`, or `json=` data. They expand during planning into normal test cases before grep, tag filtering, `only`, retries, repeats, sharding, reports, traces, and per-test GIF paths are calculated.

Runner declarations can include report annotations with `owner=`, `issue=`, `link=`, `requirement=`, `note=`, or `annotation.<type>=<description>`. Suite annotations cascade to child tests and are written to JSON, HTML, and JUnit reports.

Runner declarations can also include initial variables as `var.<name>=<value>`. Suite variables cascade to child tests, and test-level variables override suite values. Command-line `--var` and `--env` values are injected before declaration variables, so a test declaration can intentionally override a command-provided default.

Runner declarations can set per-test GIF defaults with `gifQuality=`, pointer movement/timing declarations, `gif`-prefixed framing declarations including `gifSmartCrop=`, `gifSplitTabs=`, `gifTargetZoom=`, `gifTargetZoomThreshold=`, `gifPagePosition=`, and `gifTabContext=`. `describe` / `suite` / `context` values cascade to child tests; test values override individual properties while unrelated command-line defaults remain intact. These defaults affect command-level `-gif` artifacts and are validated even when GIF output is disabled.

Artifact declarations control which command-level test GIFs are retained. `gif=always` is the default, `gif=onFailure` retains all attempts only when the test finally fails, `gif=onRetry` retains failed attempts but removes a passing attempt, and `gif=off` disables command-level capture for that test. `gifSampleRate=<n>` records the first selected test and every nth test after it. `gifCleanPassed=true` removes a passing GIF only after reports and traces are written. These declarations require `-gif` to create whole-test artifacts and do not suppress explicit `gif` / `gifIfChanged` / `gifOnFailure` blocks authored inside a script.

Relative navigation targets can be resolved with command-line `--base-url` or declaration `baseUrl=` / `baseURL=`. Suite base URLs cascade to child tests, and test-level base URLs override suite and command values.

## Options

- `--gif <directory>` / `-gif <directory>`: Record visual evidence for the entire execution of each test. GIF is the default; `--record-format` selects APNG, WebP, or MP4.
- `--no-gif`: Privacy kill switch that disables command-level and nested script GIFs without skipping test actions. `CMG_DISABLE_GIF=1` is the environment equivalent and also accepts `true`, `yes`, or `on` case-insensitively.
- `--gif-retention <all|failed|none|onRetry|days:n>`: Coarse retention policy. `all` keeps every attempted command GIF, `failed` keeps attempts only when the final test fails, `onRetry` keeps failed attempts, and `none` disables command-level capture while leaving explicit focused blocks active. `days:n` keeps the current run and, before execution, deletes complete GIF artifact families older than `n` days from the selected `--gif` directory only. Existing `always`, `onFailure`, and `off` spellings remain accepted.
- `--gif-on-failure`: Alias-style intent flag for `--gif-retention onFailure`. It cannot be combined with `--gif-retention` or `--gif-on-retry`.
- `--gif-on-retry`: Alias-style intent flag for `--gif-retention onRetry`. It cannot be combined with `--gif-retention` or `--gif-on-failure`.
- `--gif-sample-rate <n>`: Record the first selected test and every nth test after it. Must be at least `1`; defaults to `1`.
- `--gif-clean-passed`: Delete command-level GIF families for passing tests after reports and traces are written. Explicit focused block artifacts are preserved.
- `--gif-quality <archival|highest|high|medium|low>`: Recording quality for `--gif`; format-specific encoding changes never alter browser or virtual-pointer events. Defaults to `highest`.
- `--record-format <gif|apng|webp|mp4>`: Per-test whole-run artifact format. Generated names use `.gif`, `.apng`, `.webp`, or `.mp4`.
- `--record-ffmpeg <path>`: FFmpeg executable for MP4 output; fallback order is `CMG_FFMPEG`, then `ffmpeg` on `PATH`.
- `--gif-dither <none|floyd-steinberg|bayer|atkinson|sierra>`: Override the quality preset's dithering algorithm for every command-level test GIF.
- `--gif-palette <global|local|adaptive>`: Override the GIF color table. `adaptive` currently uses frame-local tables.
- `--gif-colors <2..256>`: Override the maximum GIF palette size.
- `--gif-background <color>`: Flatten transparent pixels in every whole-run GIF onto this CSS color.
- `--gif-gradient-mode <smooth|text>`: Prefer smooth-gradient or crisp-text defaults. Explicit encoder options still win.
- `--gif-high-contrast-palette`: Increase contrast and saturation for accessibility review. This intentionally changes source colors.
- `--gif-redact <selector>`: Repeatable solid-mask selector for every command-level test GIF.
- `--gif-mask <selector>`: Repeatable alias-style solid mask.
- `--gif-blur <selector>`: Repeatable blur selector; combines with solid-mask defaults.
- `--gif-auto-redact <passwords|tokens|emails|payment|privacy|none>`: Automatic whole-run privacy masking. `privacy` combines password, token, email, and payment-card-like detection. Defaults to `passwords`; `sensitive` remains an alias for `tokens`.
- `--gif-redaction-safety <standard|strict>`: Strict mode blocks capture when sensitive content remains visible.
- `--keep-frames <directory>`: Keep exact pre-encoding PNG frames. Each test writes to `<directory>/<gif-name>/frame-NNNN.png`, including retry suffixes, so parallel tests do not overwrite one another.
- `--gif-crop <selector-or-rich-locator>`: Clip each test GIF frame to current target bounds.
- `--gif-crop-padding <0..2000>`: Add CSS-pixel context around `--gif-crop`; requires `--gif-crop`.
- `--gif-smart-crop <true|false|widthxheight>`: Follow each test's virtual pointer and active element with stable crop dimensions. `true` uses `640x480`; cannot be combined with `--gif-crop`.
- `--gif-split-tabs <auto|always|none>`: Compose labeled screenshots of every tab. `always` reserves a stable two-tile canvas before popups open; `auto` expands only when multiple tabs exist. Cannot be combined with `--gif-crop` or `--gif-smart-crop`.
- `--gif-safe-area <0..500>`: Minimum target/crop margin. Defaults to `24`; use `0` to disable.
- `--gif-layout-stability <0..5000>`: Target-settling window after scroll and sticky-overlay correction. Defaults to `150` milliseconds; use `0` to disable.
- `--gif-scale <0.05..1>`: Downscale test GIF frames before quantization.
- `--gif-max-width <1..10000>`: Cap output width while preserving aspect ratio.
- `--gif-max-height <1..10000>`: Cap output height while preserving aspect ratio.
- `--gif-viewport <width>x<height>`: Temporarily use this CSS-pixel viewport for each recorded test.
- `--gif-pixel-ratio <1..4>`: Capture whole-run GIF evidence at a controlled device pixel ratio.
- `--gif-debug`: Add a frame-only diagnostics HUD and write one `<gif-name>.debug.json` sidecar per recorded test.
- `--gif-accessibility`: Enable safe keystroke labels, amplified focus evidence, accessible role/name labels, high-contrast evidence styling, and WCAG contrast warnings for every whole-run GIF.
- `--gif-event-captions`: Add privacy-safe outcome captions for network, dialog, console/page-error, download, upload, service-worker, WebSocket, and worker events in every whole-run GIF.
- `--gif-intro <text>`: Opening full-viewport title-card text for each whole-run GIF.
- `--gif-outro <text>`: Explicit final title-card text for each GIF. It takes precedence over generated result cards.
- `--gif-intro-duration <milliseconds>`: Opening title-card duration. Must be greater than zero; defaults to `1200`.
- `--gif-outro-duration <milliseconds>`: Explicit or generated final title-card duration. Must be greater than zero; defaults to `1200`.
- `--gif-result-outro`: Generate a final `Test passed`, `Test failed`, or `Test skipped` card for each test without an explicit outro.
- `--gif-no-coalesce`: Keep consecutive pixel-identical frames instead of merging their delays. Coalescing is enabled by default.
- `--gif-sample-every <1..100>`: Keep every Nth intermediate pointer/drag movement frame. Final targets and semantic evidence frames are never sampled.
- `--gif-budget <size>`: Target a maximum encoded size using units such as `500KB` or `2MB`. CMG first tries the requested settings, then lower quality presets, then bounded downscales. If no candidate meets the budget, it retains the smallest candidate and reports `budgetMet=false`.
- `--no-gif-budget-quality-fallback`: Preserve the requested quality while enforcing `--gif-budget`.
- `--no-gif-budget-downscale`: Preserve captured dimensions while enforcing `--gif-budget`.
- `--gif-narration <true|false|path>`: Write a UTF-8 narration sidecar for every recorded test. `true` uses `<gif-name>.narration.txt`.
- `--gif-alt-text <template>`: Set whole-run alt text with `{name}`, `{steps}`, `{duration}`, and `{outcome}` placeholders.
- `--gif-description <text>`: Set a human-written description for timelines and JSON/HTML reports.
- `--pointer-contrast <auto|fixed>`: Adapt an uncolored pointer to its page background. Defaults to `auto`.
- `--pointer-callout <auto|always|none>` / `--pointer-callout-threshold <8..100>`: Configure target callouts. Auto mode defaults to targets smaller than `24px` in either dimension.
- `--target-zoom <auto|always|none>` / `--target-zoom-threshold <8..100>`: Configure capture-only tiny-target zoom. Auto mode defaults to targets smaller than `24px` in either dimension.
- `--page-position <auto|always|none>`: Configure the capture-only viewport rail. Auto mode displays it on pages taller than 1.5 viewports.
- `--tab-context <auto|always|none>`: Configure the capture-only active-title/tab-count badge. Auto mode displays it when multiple tabs exist.
- `--no-pointer-focus-pulse`: Disable post-action focused-control evidence.
- `--pointer-idle <pulse|none>` / `--pointer-idle-threshold <100..60000>`: Configure long-hold pointer evidence. Defaults to a pulse after `1200ms`.
- `--no-pointer-teleport-marker`: Disable origin/path evidence for instant moves.
- `--mouse-down-hold <0..60000>`: Pressed-pointer evidence hold after `mouseDown`. Defaults to `500ms`.
- `--pointer-duration <milliseconds>`: Default virtual pointer movement duration for command-level `--gif` recordings. Must be zero or greater.
- `--pointer-speed <slow|normal|fast|instant|multiplier>`: Default virtual pointer speed for command-level `--gif` recordings. Multipliers use the `1.5x` form. DSL block and action options can still override this.
- `--pointer-easing <linear|ease-in|ease-out|ease-in-out|spring>`: Default virtual pointer easing for command-level `--gif` recordings.
- `--pointer-path <auto|direct|arc|manhattan|avoid-target|avoid-center>`: Default virtual pointer route. Defaults to geometry-aware `auto`.
- `--drag-path <auto|direct|arc|manhattan|avoid-target|avoid-center>`: Held-pointer route; inherits the pointer path when omitted.
- `--pointer-theme <arrow|hand|dot|ring|branded|touch>`: Default virtual pointer theme for command-level `--gif` recordings.
- `--pointer-color <css-color>`: Default virtual pointer color for command-level `--gif` recordings. Pass one CSS color value, not a CSS declaration.
- `--pointer-size <8..96>`: Default virtual pointer size in CSS pixels for command-level `--gif` recordings.
- `--pointer-shadow <none|light|medium|strong>`: Default virtual pointer shadow strength for command-level `--gif` recordings.
- `--show-pointer <true|false|auto>`: Default virtual pointer visibility for command-level `--gif` recordings. Defaults to `auto`, which currently shows the pointer for pointer-aware frames. Use `false` to capture frames without the DOM pointer; DSL blocks and child actions can override with `showPointer=`.
- `--gif-reduced-motion`: Removes default pointer travel animation and uses linear/static click evidence for every whole-run GIF. Explicit pointer durations still override it.
- `--gif-high-contrast-pointer`: Uses the large yellow high-contrast ring pointer for every whole-run GIF. Explicit pointer visual options override individual preset properties.
- `--caption-style <subtle|teaching|qa|bug-report|compact>`: Default caption style for command-level `--gif` recordings.
- `--caption-size <normal|large|x-large>`: Default caption text size for command-level `--gif` recordings.
- `--auto-captions`: Automatically narrate supported visual actions in each command-level test GIF.
- `--caption-template <template>`: Whole-run automatic-caption template; supplying it enables automatic captions. Supports `{action}`, `{selector}`, `{target}`, `{line}`, `{arguments}`, `{step}`, and `{assertion}`. Unknown placeholders fail before browser connection or `--list` output.
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
- `--gif-timeline <file-or-directory>`: Optional explicit JSON timeline destination for GIF recordings. With `cmg run --gif`, pass a directory so each test writes `<gif-name>.timeline.json`. JSON/HTML reports enable the default sidecar automatically when this option is omitted.
- `--gif-warn-size <size>`: Emit a stdout warning when a recorded GIF exceeds this size. Accepts bytes or `KB`, `MB`, and `GB` suffixes, for example `500KB`.
- `--gif-max-size <size>`: Fail a test when a recorded GIF exceeds this size. Accepts bytes or `KB`, `MB`, and `GB` suffixes, for example `500KB`.
- `--gif-max-duration <duration>`: Fail a test when a recorded GIF exceeds this duration. Accepts plain milliseconds or `ms`, `s`, and `m` suffixes, for example `2500ms`, `10s`, or `1m`.
- `--config <file>`: JSON run config file. CLI options override config values.
- `--project <name>`: Named project from the run config. Project values override global config values, and CLI options override both.
- `--report-json <file>`: Write a JSON test report. When GIF recording is active, CMG also retains timeline sidecars needed for frame-level report evidence.
- `--report-html <file>`: Write an HTML test report. When GIF recording is active, CMG also retains timeline sidecars needed for frame-level report evidence.
- `--report-junit <file>`: Write a JUnit XML test report.
- `--trace <directory>`: Write per-test trace JSON files.
- `--grep <text>`: Run tests whose names contain the text.
- `--tag <tag>`: Run tests with a matching `tag=` option.
- `--retries <count>`: Retry failed tests this many times.
- `--max-failures <count>`: Stop scheduling tests after this many failed tests. `0` disables fail-fast behavior.
- `--repeat-each <count>`: Run each selected test this many times. Values below `1` are treated as `1`.
- `--list`: List selected tests without connecting to a browser or running actions.
- `--shard <index/count>`: Run a deterministic shard, for example `1/3`.
- `--timeout <milliseconds>`: Default timeout for timeout-capable waits, event waits, downloads, network waits, worker waits, tab waits, API requests, and assertions that do not set `timeout=`.
- `--navigation-timeout <milliseconds>`: Default timeout for navigation actions and navigation waits.
- `--assertion-timeout <milliseconds>`: Default timeout for assertions. Overrides `--timeout` for assertion actions.
- `--base-url <url>`: Absolute base URL used to resolve relative `navigate`, `goto`, `visit`, `openTab`, and `newContext url=` targets in every selected test.
- `--browser-port <port>`: Remote debugging port for the browser instance used by this run. Use this with browsers launched through `cmg browser --port <port> launch`.
- `--auto-launch`: Launch the selected browser automatically when no CMG-controlled browser is running. The launch uses the selected browser and `--browser-port` value with the same defaults as `cmg browser launch`.
- `--headless`: Launch the selected browser in headless mode when `--auto-launch` starts a browser. Chrome and Edge receive `--headless=new`; Firefox receives `--headless`.
- `--browser-idle-timeout <duration>`: Opt into a renewable idle lease for the selected CMG-owned headless browser. Accepts positive `ms`, `s`, `m`, or `h` durations. When auto-launching, this requires `--headless`.
- `--no-browser-idle-cleanup`: Disable an existing lease without closing the browser. It cannot be combined with `--browser-idle-timeout`.
- `--var <name=value>`: Initial variable for every selected test. Can be repeated. Later entries with the same name replace earlier entries.
- `--env <name=value>`: Alias for `--var`, intended for CI and agent-provided environment values.
- `--chrome`: Use Chrome. This is the default.
- `--edge`: Use Microsoft Edge.
- `--firefox`: Use Firefox.

## Output

Stdout prints one parseable line per test:

```text
TEST PASS <name>
TEST FAIL <name>
TEST SKIP <name>
GIF_FRAMES path="<JSON-escaped-absolute-directory>" count=<frames>
GIF_FAILURE_CAPTION <line> action="<action>" status=captured
GIF_WARN_SIZE test="<name>" path="<gif-path>" sizeBytes=<bytes> thresholdBytes=<bytes>
GIF_WARN_PALETTE test="<name>" path="<gif-path>" paletteColors=<count-or->256> thresholdColors=240 palette=<mode>
GIF_WARN_COLOR_PROFILE path="<gif-path>" profileChanges=<count>
GIF_WARN_POINTER_PROMOTION overlays=<comma-separated-overlay-ids> reason=top-layer-promotion-failed
GIF_RETENTION test="<name>" path="<absolute-path>" action=deleted mode=<always|onFailure|onRetry|off>
GIF_CLEAN_PASSED test="<name>" path="<absolute-path>" status=deleted
GIF_RETENTION_WARN tests=<count> threshold=20 reason=large-suite suggestion=--gif-on-failure
GIF_MAX_SIZE test="<name>" path="<gif-path>" sizeBytes=<bytes> thresholdBytes=<bytes>
GIF_MAX_DURATION test="<name>" path="<gif-path>" durationMs=<milliseconds> thresholdMs=<milliseconds>
RUN STOP maxFailures=<count>
TEST LIST <run|skip> <name>
BROWSER_IDLE_LEASE status=<scheduled|disabled> browser=<browser> port=<port> pid=<pid> ownership=cmg idleTimeoutMs=<milliseconds> deadline=<ISO-8601|none> reason=<reason>
```

Failures may include action output before the failing test line. Declaration-skipped tests do not run actions or produce GIFs. Runtime `skip "reason"` stops the current test, preserves output and GIF frames captured before the skip, and records `TEST SKIP <name>`. `GIF_FRAMES` identifies each retained-frame directory and count; it is emitted only when recording and `--keep-frames` are both active. `GIF_DEBUG <path>` identifies a per-frame diagnostics sidecar created by `--gif-debug`. Encoder/debug flags without `--gif` do not capture frames or inject a virtual pointer. `GIF_WARN_SIZE` lines are emitted only when `--gif-warn-size` is set and a recorded GIF exists above the threshold; they do not change the run exit code. `GIF_WARN_PALETTE` lines are emitted automatically when a recorded GIF uses at least 240 decoded colors or exceeds the 256-color counting cap, which tells agents that the artifact may show color pressure or dithering. `GIF_MAX_SIZE` and `GIF_MAX_DURATION` lines are emitted when their matching guard is set and a recorded GIF exceeds the threshold; the test is marked failed and the run exits `1`. `RUN STOP maxFailures=<count>` means `--max-failures` stopped the run after the threshold was reached. `TEST LIST` lines are emitted by `--list` and show the selected schedule without browser execution. Stderr contains the final error when one is available.

`GIF_CAPTURE_STATS` is emitted for every recorded test with source/retained frame counts, duplicate and sampled counts, blank-frame count, peak retained pixel bytes, preprocessing milliseconds, and requested/final budget decisions. `GIF_WARN_UNCHANGED` and `GIF_WARN_BLANK` are non-failing artifact-quality warnings. Timeline JSON stores the same values under `captureDiagnostics`; each timeline/report evidence step also records captured frame count, encoded duration, and estimated retained RGBA bytes. Age cleanup emits one `GIF_RETENTION_CLEANUP path="..." ageDays=<n> action=deleted` line per family.

Each retained artifact emits `GIF_REPRODUCE path="..." command="..."`. Commands preserve the selected browser, browser port, project, source file, and test grep. Command-level artifacts include `--gif <directory>`; focused DSL artifacts omit it so their `gif` block remains authoritative.

Requested narration files emit `GIF_NARRATION <absolute-path>`. Runner declarations are `gifNarrationSidecar=`, `gifAltText=`, `gifDescription=`, `gifFormat=`, and `gifFfmpeg=`; suite values cascade and test values override them. Root/project `gifSettings.narrationSidecar`, `altText`, `description`, `format`, and `ffmpegPath` provide coarse defaults, with CLI and DSL declarations remaining more specific.

Retention treats conventional narration files and custom narration paths inside the selected GIF artifact directory as part of the GIF family. For safety, age cleanup does not follow a timeline path outside that directory; externally located narration is caller-managed.

`GIF_RETENTION` reports artifacts removed while retry/failure policy is resolved. `GIF_CLEAN_PASSED` is emitted after report and trace generation. Both are stdout diagnostics and do not change the exit code. Reports receive the retained artifact metadata before `gifCleanPassed=true` cleanup occurs.

`GIF_RETENTION_WARN` is an advisory stdout line emitted when command-level `--gif` would retain every selected test in a file and the post-filter, post-shard selection exceeds 20 tests. It is also emitted by browser-free `--list`. Use `--gif-on-failure`, `--gif-on-retry`, `--gif-sample-rate`, `--gif-clean-passed`, or equivalent suite/test declarations when that volume is unintended. The warning does not change selection, recording, or exit status.

`GIF_DISABLED source=<cli|environment>` confirms the privacy kill switch is active. Explicit recording blocks emit `GIF_SKIPPED <line> status=skipped reason=recording-disabled source=<cli|environment>` but execute their children. Suppression creates no GIF paths, screenshots, recording UI, or virtual pointer and silences retention-volume warnings. It does not change test status or exit code.

`GIF_FAILURE_CAPTION` confirms that CMG wrote an explicit visual failure explanation into the partial test GIF. The full failure reason remains available in stderr and structured reports.
Parameterized tests print and report their expanded names, for example `TEST LIST run opens profile`. Project runs include the project name in brackets, for example `TEST LIST run [firefox-smoke] checkout`.

When a step fails, stderr also includes:

```text
STEP FAIL line=<line> action=<action> reason=<reason>
```

When a file cannot be parsed, imported, or planned into a runnable test, stdout still prints `TEST FAIL <file>` and stderr includes:

```text
TEST ERROR <file> reason=<reason>
```

Invalid run config files fail before browser connection or test listing. Stderr names the config problem, for example `Run config option 'retries' must be an integer.` or `Run config '<path>' was not valid JSON. ...`.

If no selected CMG browser is running, stderr tells the caller which launch command to run, for example `No CMG-controlled Chrome instance is running. Run 'cmg browser launch' first.` Use `--auto-launch` when the runner should start the selected browser automatically, and add `--headless` when that auto-launched browser should not open a visible window.

`--browser-idle-timeout` and config `browserIdleTimeout` are opt-in. During the run, CMG renews the lease in the background so a long test cannot expire its own browser. Cleanup remains disabled when neither is set. Config `noBrowserIdleCleanup: true` matches `--no-browser-idle-cleanup`; CLI values override config. Conflicting enable/disable settings fail before launch.

Run config supports `gifRetention` (`always`, `onFailure`, `onRetry`, or `off`), positive integer `gifSampleRate`, and boolean `gifCleanPassed`. Explicit CLI retention values override config. Suite/test declarations then override the effective run default one property at a time.

Root config and individual projects may define a `gifSettings` object. Supported properties are `quality`, `format`, `ffmpegPath`, `pointerDuration`, `pointerSpeed`, `pointerEasing`, `pointerPath`, `dragPath`, `clickPulse`, `fps`, `frameDelay`, `crop`, `cropPadding`, `smartCrop`, `splitTabs`, `scale`, `maxWidth`, `maxHeight`, `viewport`, `pixelRatio`, `safeArea`, `layoutStability`, `targetZoom`, `targetZoomThreshold`, `pagePosition`, `tabContext`, `captionStyle`, `captionPosition`, `captionSeverity`, `captionSize`, `autoCaptions`, `captionTemplate`, `narrationSidecar`, `altText`, `description`, string arrays `redact`, `mask`, and `blur`, plus `autoRedact` and `redactionSafety`. Project properties overlay root properties individually; explicit CLI options overlay project properties individually; suite/test declarations and DSL recording/action overrides remain more specific. Unknown properties and invalid types fail during config loading; invalid values fail before browser connection or `--list` output with the responsible setting in stderr.

```json
{
  "gifSettings": { "quality": "medium", "pointerSpeed": "slow", "fps": 8 },
  "projects": [{
    "name": "chrome-visual",
    "browser": "chrome",
    "gifSettings": { "quality": "highest", "frameDelay": 120 }
  }]
}
```

Reports and traces include per-test status, output, and per-step diagnostics so agents can explain why a run failed. The HTML report starts with total/pass/fail/skip counts, groups failed tests by their reported reason, and presents all retained GIFs as an ordered run filmstrip. JSON and HTML `steps` contain public executed runtime steps only; planned placeholders and generated internal evaluate/actionability/locator steps are omitted. Public report step sequences are contiguous per test, and output payload lines are renumbered to match their parent step. JSON test entries include `browser`, nullable `browserPort`, `gifReproductionCommands[]` objects with `gifPath` and `command`, and `gifDiagnostics[]` objects with `severity` and `message`. Invalid recording settings are `error` diagnostics; retained GIF authoring/runtime warnings are `warning` diagnostics. HTML reports show the same diagnostics and reproduction commands. JSON step entries include separate `sequence`, `lineNumber`, `context`, and `action` fields; agents should use those fields instead of parsing human stdout strings. When a step captured GIF frames, its `gifEvidence` array includes the artifact and timeline paths, zero-based start/end frame indexes, start/end times, and an optional failure-frame index. JSON `gifMetadata` entries include `timelinePath`, nullable `narrationPath`, rendered `altText`, authored `description`, quality, frames, duration, approximate FPS, dimensions, size, palette details, transparency, and repeat metadata. HTML reports use authored alt text on previews, link narration sidecars, and show descriptions. They also link each visual step to an embedded static start frame and show a static final diagnostic frame for failures. Traces keep lower-level raw diagnostics. JUnit reports emit `<skipped>` nodes for declaration-skipped tests and runtime skips, plus `cmg.gif.narrationPath`, `cmg.gif.altText`, and `cmg.gif.description` properties when present.
Report annotations are emitted as `annotations` in JSON, visible list items in HTML, and JUnit `<property name="cmg.annotation.<type>" ... />` entries. JUnit GIF properties use `cmg.gif.path` for one artifact or `cmg.gif.path.1`, `cmg.gif.path.2`, and so on for multiple artifacts. Failed tests with an inspectable GIF also include `cmg.gif.failureFrameIndex`, using the final frame index as the failure-state frame.

Each JSON `gifMetadata` entry also has `format`. GIF, APNG, and WebP include decoded frame, duration, dimension, size, true-color/palette, transparency, and repeat data. HTML uses animated image previews for those formats and `<video controls>` for MP4. Format-specific reproduction commands include `--record-format`.

## GIF Behavior

GIF recording is optional.

- With `--gif` or `-gif`, CMG records the whole execution of each test.
- `--gif-quality` defaults to `highest`, using CMG's most color-faithful palette matching and dithering. Use `high`, `medium`, or `low` to trade color fidelity for smaller/faster GIF artifacts.
- Pointer motion, styling, contrast, target callout, focus, idle, teleport, pressed-state, and visibility options set whole-test virtual-pointer defaults only when `--gif` is active.
- `--caption-style`, `--caption-position`, `--caption-severity`, and `--caption-size` set whole-test caption defaults for `caption`, `showMessageBar`, `step`, and flattened `gif` / `recordVideo` / `screencast` block captions.
- `--gif-accessibility` enables safe keyboard labels, focus and accessible-name evidence, high-contrast styling, and targeted-control contrast warnings for every retained frame. It is inert without `--gif`.
- `--gif-event-captions` summarizes event outcomes without copying console text, page-error stacks, request URLs, WebSocket payloads, worker expressions/results, download paths, or upload filenames into the GIF. It is inert without `--gif`.
- Title-card flags are also inert without `--gif`. Explicit `--gif-outro` text wins over `--gif-result-outro`; generated cards use the real test outcome after soft failures, runtime failures, or skips are resolved.
- `--click-pulse` sets the whole-test click/tap/drop pulse style when `--gif` is active.
- `--gif-hold-after-action` sets the whole-test post-action hold duration when `--gif` is active.
- `--pointer-pre-click-hold` and `--pointer-post-click-hold` set whole-test click/tap settle and post-pulse hold durations when `--gif` is active.
- `--gif-hold-after-navigation` and `--gif-hold-after-assertion` set whole-test navigation and assertion hold durations when `--gif` is active.
- `--gif-hold-on-failure` captures one extra final-state hold frame before writing a failed test GIF.
- `--gif-fps` sets the whole-test frame rate when `--gif` is active.
- `--gif-frame-delay` sets the whole-test frame delay and overrides `--gif-fps`.
- `--gif-timeline` writes metadata JSON sidecars and emits `GIF_TIMELINE <path>` in the per-test output after each GIF is saved.
- `--report-json` or `--report-html` automatically enables the default `<gif-name>.timeline.json` sidecar for recorded GIFs when `--gif-timeline` was not specified. An explicit `--gif-timeline` path still wins.
- `--gif-warn-size` emits `GIF_WARN_SIZE` stdout lines for command-level and block-level GIF files whose final size exceeds the threshold.
- `--gif-max-size` emits `GIF_MAX_SIZE` stdout lines and fails tests whose recorded artifacts exceed the threshold.
- `--gif-max-duration` emits `GIF_MAX_DURATION` stdout lines and fails tests whose recorded artifacts exceed the threshold.
- Palette diagnostics are automatic. A recorded artifact near the GIF color limit emits `GIF_WARN_PALETTE` with the decoded color count, threshold, and palette mode.
- When command-level GIF recording is active, script-level `gif { ... }`, `recordVideo { ... }`, and `screencast { ... }` blocks do not create nested recordings; their actions are flattened into the whole-test GIF.
- Without command-level GIF recording, script-level `gif "name" { ... }`, `recordVideo "name" { ... }`, or `screencast "name" { ... }` records only the wrapped block.
- Without command-level GIF recording or an active script-level recording block, CMG does not inject the virtual pointer. Recording-only actions such as `pauseGif`, `moveMouse`, `recordCheckpoint`, `showPointer`, and `hidePointer` are skipped and do not create pointer frames or timeline entries.
- If `--max-failures` stops the run, GIFs and reports include only tests that actually ran before the stop.
- With `--repeat-each`, each repeat is a separate scheduled test with a distinct name such as `checkout [repeat 2/3]`, so per-test GIFs, traces, reports, retries, and sharding remain deterministic.
- Command-level GIF filenames include the selected project (when present), browser, shard identity (when sharded), sanitized test/repeat name, and `-attempt-N` for retries after the first. For example: `firefox-smoke-firefox-shard-2-of-4-checkout--repeat-2-3--attempt-2.gif`. This prevents parallel matrix jobs that share an artifact directory from overwriting one another.

All recorded actions use CMG's virtual pointer, pointer/mouse event dispatch, captions, and drag ghost behavior. Selector actions accept CMG rich locators and provider-style aliases. Every non-CSS locator resolves to the same temporary element marker used by the GIF recorder, so virtual pointer movement, pointer events, drag ghosts, and captions remain aligned with the chosen element.

Actions, locators, control flow, loops, macros, scoped variables, `recording` / `withRecording` scoped GIF defaults, and `gif` blocks are shared with direct browser-control scripts unless a reference page says otherwise. Start with the [action index](../scripting/action-index.md), then use the [detailed action reference](../scripting/actions.md) for options and examples.

Invalid encoder values fail before browser connection or test scheduling and name the option, including invalid `background=` colors and `gradientMode=` values.

Invalid framing values use the same pre-browser failure path and name `cropPadding=`, `safeArea=`, `layoutStability=`, `scale=`, `maxWidth=`, or `maxHeight=`. A crop selector missing during a test fails that test with the selector resolution reason.

`contains "text"` and `notContains "text"` check the page body. `contains "<selector>" "text"`, `containsText`, `waitForText`, `notContainsText`, and the negative text aliases check a selector or rich locator and accept `timeout=<milliseconds>`, `match=contains|exact|regex`, and `ignoreCase=true`. Successful text checks emit the normal test/step pass output; failed checks include the expected and actual text in the step failure reason.

## Exit Codes

- `0`: All tests passed.
- `1`: At least one test failed, a GIF exceeded `--gif-max-size` or `--gif-max-duration`, no script files matched, the selected browser is invalid, `--browser-port` is outside `1..65535`, the selected `--project` is missing or invalid, the selected browser is not running, or `--auto-launch` could not start it.
- `1`: A GIF encoder option is invalid or `--gif-colors` is outside `2..256`.

## Examples

```powershell
cmg browser launch
cmg run demo-scripts\20-runner-flow.cmgscript
cmg run tests\flows --gif artifacts\gifs
cmg run tests\flows --gif artifacts\gifs --gif-quality highest
cmg run tests\flows --gif artifacts\gifs --gif-dither sierra --gif-palette local --gif-colors 192 --keep-frames artifacts\source-frames
cmg run demo-scripts\183-gif-debug-runner.cmgscript --gif artifacts\debug --gif-debug
cmg run tests\flows --gif artifacts\gifs --gif-crop "#stage" --gif-crop-padding 24 --gif-scale 0.75 --gif-max-width 700
cmg run demo-scripts\228-gif-auto-pointer-path-runner.cmgscript --gif artifacts\paths --pointer-path auto --drag-path avoid-target
cmg run tests\flows --gif artifacts\gifs --pointer-duration 600 --pointer-easing spring
cmg run tests\flows --gif artifacts\gifs --pointer-theme ring --pointer-color "#dc2626" --pointer-size 44 --pointer-shadow strong
cmg run tests\flows --gif artifacts\gifs --show-pointer false
cmg run demo-scripts\181-gif-accessible-presets-runner.cmgscript --gif artifacts\accessible --gif-reduced-motion --gif-high-contrast-pointer
cmg run demo-scripts\185-gif-contrast-captions-runner.cmgscript --gif artifacts\accessibility-review --gif-accessibility --caption-size large
cmg run demo-scripts\187-gif-event-captions-runner.cmgscript --gif artifacts\event-evidence --gif-event-captions
cmg run demo-scripts\189-gif-result-cards-runner.cmgscript --gif artifacts\result-cards --gif-intro "Checkout review" --gif-result-outro
cmg run demo-scripts\191-gif-capture-efficiency-runner.cmgscript --gif artifacts\efficient --gif-sample-every 3 --gif-timeline artifacts\timelines

cmg run demo-scripts\212-gif-retention-runner.cmgscript --gif artifacts\retention --retries 1 --report-json artifacts\retention.json
cmg run demo-scripts\213-gif-retention-cli-runner.cmgscript --gif artifacts\retry-evidence --gif-on-retry --retries 1
cmg run tests\flows --gif artifacts\gifs --caption-style qa --caption-position bottom --caption-severity success
cmg run tests\flows --gif artifacts\gifs --click-pulse ripple --pointer-pre-click-hold 120 --pointer-post-click-hold 450 --gif-hold-on-failure 1800
cmg run tests\flows --gif artifacts\gifs --gif-timeline artifacts\timelines
cmg run checkout.cmgscript --report-json artifacts\checkout.json --report-html artifacts\checkout.html
cmg run checkout.cmgscript --trace artifacts\traces
cmg run tests\flows --grep checkout --tag smoke --retries 2 --shard 1/3
cmg run tests\flows --max-failures 1
cmg run tests\flows --repeat-each 3
cmg run tests\flows --list --grep checkout
cmg run tests\flows --timeout 10000 --navigation-timeout 15000 --assertion-timeout 5000
cmg run tests\flows --var user=Ada --env mode=demo
cmg run tests\flows --base-url https://example.test/app/
cmg browser --port 9333 launch --headless
cmg run tests\flows --browser-port 9333
cmg run demo-scripts\147-run-config.cmgscript --config demo-scripts\run-config.example.json --list
cmg run demo-scripts\147-run-config.cmgscript --config demo-scripts\run-config.example.json --project chrome-smoke
cmg run demo-scripts\147-run-config.cmgscript --config demo-scripts\run-config.example.json --project firefox-smoke
cmg run tests\flows --auto-launch
cmg run tests\flows --auto-launch --headless
cmg run tests\flows --auto-launch --headless --browser-idle-timeout 45m
```

Example config:

```json
{
  "gif": "../demo-output/runner-gifs",
  "trace": "../demo-output/traces",
  "reportJson": "../demo-output/run-report.json",
  "reportHtml": "../demo-output/run-report.html",
  "grep": "config",
  "tag": "smoke",
  "retries": 1,
  "maxFailures": 2,
  "repeatEach": 1,
  "shard": "1/1",
  "timeout": 10000,
  "navigationTimeout": 15000,
  "assertionTimeout": 5000,
  "baseUrl": "https://example.test/app/",
  "browserIdleTimeout": "45m",
  "noBrowserIdleCleanup": false,
  "gifRetention": "onFailure",
  "gifSampleRate": 1,
  "gifCleanPassed": false,
  "gifSettings": {
    "sizeBudget": "750KB",
    "budgetQualityFallback": true,
    "budgetDownscaleFallback": true,
    "narrationSidecar": "true",
    "altText": "{name}: {steps} automation steps, {outcome}",
    "description": "Automated browser evidence."
  },
  "projects": [
    {
      "name": "chrome-smoke",
      "browser": "chrome",
      "baseUrl": "https://chrome.example.test/app/",
      "tag": "smoke",
      "variables": {
        "browserName": "chrome"
      }
    },
    {
      "name": "firefox-smoke",
      "browser": "firefox",
      "baseUrl": "https://firefox.example.test/app/",
      "tag": "smoke",
      "variables": {
        "browserName": "firefox"
      }
    }
  ],
  "variables": {
    "tenant": "demo",
    "mode": "config",
    "browserName": "default"
  }
}
```

Use runner options on test declarations for provider-style focus and skip behavior:

```text
test "checkout" only=true {
  click "#pay"
}

test "legacy flow" skip=true reason="Disabled until the legacy page is removed" {
  click "#legacy"
}

test.only "debug checkout" {
  click "#pay"
}

test.fixme "broken checkout"
test.todo "add refund coverage"

test.slow "slow checkout" {
  expectText "#status" "Saved"
}

describe.skip "legacy area" {
  it "old case" {
    click "#old"
  }
}

describe.slow "slow area" {
  it "inherits slow timeout policy" {
    waitForSelector "#eventual"
  }
}

test.each "opens ${page}" as=page values="profile,checkout" tag=smoke {
  click "#${page}"
}

test.each "opens ${case.name}" as=case json="[{\"name\":\"Profile\",\"selector\":\"#profile\"}]" {
  click "${case.selector}"
}

test "annotated checkout" owner=qa issue="BUG-7" annotation.requirement="REQ-1" {
  click "#checkout"
}

describe "tenant flow" var.tenant=demo {
  test "uses suite variables" {
    expect (${tenant} == "demo")
  }

  test "overrides suite variables" var.tenant=staging {
    expect (${tenant} == "staging")
  }
}

describe "relative navigation" baseUrl="https://example.test/app/" {
  test "opens profile" {
    navigate "profile"
  }
}
```

When any selected test has `only=true` or a `.only` declaration, `cmg run` runs only focused tests. `skip=true`, `.skip`, `.fixme`, and `.todo` record `TEST SKIP <name>` and a skipped report entry without running actions. Suite-level focus and skip declarations cascade to child tests. For script structure and style guidance, see [syntax](../scripting/syntax.md) and the [style guide](../scripting/style-guide.md).
`slow=true` or `.slow` scales inherited default wait, navigation, and assertion timeouts for that test by `3x`; `slow=<number>` uses a custom multiplier. Explicit action-level `timeout=` still wins.
