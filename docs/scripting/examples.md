# CMG Examples

Use this page as a learning path. It starts with small scripts you can run immediately, then points to deeper examples when you need a specific capability.

For the full catalogue of advanced examples, see the [cookbook reference](cookbook-reference.md). Runnable files live in [`../../demo-scripts/`](../../demo-scripts/).

## Start Here

| Goal | Read | Run |
| --- | --- | --- |
| Make a first GIF | [First GIF](#first-gif) | `demo-scripts\01-dialog-flow.cmgscript` |
| Write a first test | [First Test](#first-test) | `demo-scripts\20-runner-flow.cmgscript` |
| Show pointer behavior | [Visual Evidence](#visual-evidence) | `demo-scripts\10-css-hover-states.cmgscript` |
| Tune GIF quality | [Visual Evidence](#visual-evidence) | `demo-scripts\148-gif-quality.cmgscript` |
| Choreograph GIF pointer movement | [Visual Evidence](#visual-evidence) | `demo-scripts\149-gif-pointer-choreography.cmgscript` |
| Protect sensitive GIF evidence | [GIF Privacy](#gif-privacy) | `demo-scripts\174-gif-redaction.cmgscript` |
| Show touch pointer and hide/show controls | [Visual Evidence](#visual-evidence) | `demo-scripts\155-touch-pointer-visibility.cmgscript` |
| Style the virtual pointer for evidence | [Visual Evidence](#visual-evidence) | `demo-scripts\156-gif-pointer-styles.cmgscript` |
| Reuse script logic | [Variables And Macros](#variables-and-macros) | `demo-scripts\30-control-flow-macros.cmgscript` |
| Handle failures clearly | [Failure Feedback](#failure-feedback) | `demo-scripts\52-explicit-fail.cmgscript` |
| Tune one slow section | [Scoped Timeouts](#scoped-timeouts) | `demo-scripts\134-scoped-timeouts.cmgscript` |
| Run the same test for data rows | [Parameterized Tests](#parameterized-tests) | `demo-scripts\136-parameterized-tests.cmgscript` |
| Add report metadata | [Report Annotations](#report-annotations) | `demo-scripts\138-report-annotations.cmgscript` |
| Parameterize scripts from outside | [Initial Variables](#initial-variables) | `demo-scripts\139-cli-variables.cmgscript` |
| Use relative navigation | [Base URL](#base-url) | `demo-scripts\141-base-url.cmgscript` |

Start the browser before running direct scripts:

```powershell
cmg browser launch
```

When developing from the repository, use `dotnet run --` in place of `cmg`.

## First GIF

Create a short browser-control script:

```text
navigate "C:\Projects\CMG\index.html"
waitForElement "#openProfileDialog" timeout=5000

step "Open the profile dialog" {
  click "#openProfileDialog"
}

type "#profileName" "CMG quick start"
```

Record it:

```powershell
cmg browser control script --file first-gif.cmgscript --gif demo-output\first-gif.gif --gif-quality highest
```

The recording shows CMG's virtual pointer, real page pointer events, hover states, typed text, captions, and any captured frames before a failure.

## First Test

Use `cmg run` when the same actions should become repeatable tests with reports and optional per-test GIFs:

```text
suite "profile dialog" {
  beforeEach {
    navigate "C:\Projects\CMG\index.html"
    waitForElement "#openProfileDialog"
  }

  test "opens" tag=smoke {
    click "#openProfileDialog"
    expectVisible "#profileDialog"
    expectText "#lastDialogAction" "None"
  }
}
```

Run it with reports:

```powershell
cmg run profile.test.cmgscript --report-html demo-output\report.html --report-json demo-output\report.json
```

Add `--gif demo-output\gifs` when every selected test should produce a whole-test GIF.

Use a runner config when CI or an agent should reuse the same reports, traces, retries, variables, and selection defaults:

```powershell
cmg run demo-scripts\147-run-config.cmgscript --config demo-scripts\run-config.example.json --list
cmg run demo-scripts\147-run-config.cmgscript --config demo-scripts\run-config.example.json --var mode=cli
cmg run demo-scripts\147-run-config.cmgscript --config demo-scripts\run-config.example.json --project chrome-smoke
```

## Visual Evidence

Pointer-aware actions move the same virtual pointer that appears in GIFs. This keeps the visible pointer, browser events, hover state, screenshots, drag ghosts, and captions aligned.

```text
navigate "C:\Projects\CMG\index.html"
waitForElement "#hoverDemoCard"

step "Show the hover card" {
  hover "#hoverDemoCard"
  expectText "#hoverStateText" "Hover card active"
}
```

Run the complete hover demo:

```powershell
cmg browser control script --file demo-scripts\10-css-hover-states.cmgscript --gif demo-output\css-hover-states.gif --gif-quality highest
```

For drag evidence, run:

```powershell
cmg browser control script --file demo-scripts\07-complex-drag-flow.cmgscript --gif demo-output\complex-drag.gif
```

For click evidence, run:

```powershell
cmg browser control script --file demo-scripts\14-pointer-click-variants.cmgscript --gif demo-output\click-variants.gif
```

To compare GIF encoder presets and the block aliases, run:

```powershell
cmg browser control script --file demo-scripts\148-gif-quality.cmgscript
```

The `gif`, `recordVideo`, and `screencast` blocks all use the same CMG recorder. `quality=highest` is the default and gives the most color-faithful palette; `high`, `medium`, and `low` trade fidelity for smaller/faster artifacts.

To choreograph pointer timing and drag evidence, run:

```powershell
cmg browser control script --file demo-scripts\149-gif-pointer-choreography.cmgscript --gif demo-output\pointer-choreography.gif --pointer-duration 500 --gif-hold-after-action 700
cmg browser control script --file demo-scripts\150-gif-failure-hold.cmgscript --gif demo-output\failure-hold.gif --gif-hold-on-failure 1800 --gif-timeline demo-output\timelines
cmg browser control script --file demo-scripts\155-touch-pointer-visibility.cmgscript --gif demo-output\touch-pointer-visibility.gif
```

Recording blocks can set `pointerDuration=`, `pointerSpeed=`, `pointerEasing=`, `clickPulse=`, `pressedPointer=`, `dragTrail=`, `dragBreadcrumbs=`, `preClickHold=`, `postClickHold=`, `holdAfterAction=`, `holdAfterNavigation=`, `holdAfterAssertion=`, `holdOnFailure=`, `fps=`, `frameDelay=`, and `timeline=` as defaults. Use `recording { ... }` or `withRecording { ... }` when several actions or nested recording blocks should share the same defaults without starting a recording by themselves. Use `pauseGif <milliseconds>` for recording-only holds, `recordCheckpoint "name"` for named JSON timeline markers, and `showPointer` / `hidePointer` when a GIF needs a pointer-visible or unobstructed frame. These recording-only actions skip without injecting the virtual pointer when no GIF recording is active. If a block has child actions, such as `dragAndDrop { ... }`, the parent options are scoped defaults and each child action can override them locally. Use `--gif-timeline` or block-level `timeline=true` when reports or agents need JSON timing metadata beside the GIF.

Runner JSON/HTML reports retain timelines automatically when GIFs are enabled, so an agent can jump from a failed or repeated runtime step to its exact visual frame without adding `--gif-timeline`:

```powershell
cmg run demo-scripts\173-gif-report-frame-evidence.cmgscript --gif demo-output\report-frame-gifs --report-json demo-output\report-frame-evidence.json --report-html demo-output\report-frame-evidence.html
```

The demo intentionally exits `1` for its second test. The JSON report exposes `steps[].gifEvidence`; the HTML report embeds each visual start frame and the final failure frame.

Pointer visuals can be styled with `pointerTheme=`, `pointerColor=`, `pointerSize=`, `pointerShadow=`, and `showPointer=` on `recording`, `gif`, `recordVideo`, `screencast`, and individual pointer-aware actions. Use this when a GIF needs a ring pointer for review, a branded pointer for demos, a touch pointer for tap flows, or clean page-state frames without the DOM pointer. Command-level `--pointer-theme`, `--pointer-color`, `--pointer-size`, `--pointer-shadow`, and `--show-pointer` set whole-run defaults for `--gif` recordings.

Caption visuals can be styled with `captionStyle=`, `captionPosition=`, and `captionSeverity=` on recording scopes, recording blocks, `caption`, `showMessageBar`, and `step`. Use `qa` or `bug-report` styles for evidence review, `teaching` for onboarding demos, and `compact` when the caption should stay out of the way.

For stable screenshot evidence, mask volatile regions only in the artifact. The GIF still shows the real page and pointer choreography:

```text
setContent "<main><h1>Evidence</h1><p id='clock'>12:34:56</p><button id='save'>Save</button></main>"
screenshotPage output="demo-output\masked-evidence.png" mask="#clock" maskColor="#000000" animations=disabled caret=hide
```

Run the complete screenshot mask demos:

```powershell
cmg browser control script --file demo-scripts\143-screenshot-mask.cmgscript
cmg run demo-scripts\144-screenshot-mask-runner.cmgscript
cmg browser control script --file demo-scripts\145-screenshot-deterministic.cmgscript
cmg run demo-scripts\146-screenshot-deterministic-runner.cmgscript
```

## GIF Privacy

Record privacy-safe visual evidence without changing the application DOM or values:

```powershell
cmg browser control script --file demo-scripts\174-gif-redaction.cmgscript
cmg run demo-scripts\175-gif-redaction-runner.cmgscript
```

The direct and runner demos use `autoRedact=sensitive`, an inherited blurred email mask, a replacement account mask, automatic password/token masks, normal virtual-pointer movement, a click pulse, captions, and timeline audit metadata. The account is deliberately revealed with `unmaskGif` only after the protected action is complete.

The strict-safety demo intentionally exits `1` and writes no unsafe GIF because it disables automatic masking while a password field is visible:

```powershell
cmg browser control script --file demo-scripts\176-gif-redaction-strict-failure.cmgscript
```

Use solid or replacement masks for secrets. Blur preserves visual context but should not be treated as irreversible redaction. See [GIF Recording](gif-recording.md#privacy-and-redaction) for every option and the timeline audit shape.

## Variables And Macros

Use `set` for values and action output. The variable stores the actual payload value, not a pass/fail wrapper:

```text
set title {
  evaluate "document.title"
}

if (${title} contains "CMG") {
  caption "Loaded ${title}"
}

expect (${title} contains "CMG")
expect evaluate "document.title" contains "CMG"
softExpect (${title} contains "Dashboard") message="Dashboard title was not shown"
```

Macros are reusable, scoped blocks:

```text
macro openProfile name {
  click "#openProfileDialog"
  fill "#profileName" "${name}"
  return "${name}"
}

set savedName {
  call openProfile "Ada"
}

set labels {
  allTextContents ".command"
}

foreachJson label "${labels}" {
  caption "Command ${index}: ${label}"
}
```

Variables set inside a macro are local to that macro call. A macro can read variables from the parent tree where it was defined, and local values shadow parent values without mutating them.

## Initial Variables

Use command-line variables when an agent, CI job, or wrapper needs to supply data without editing the script:

```powershell
cmg browser control script --file demo-scripts\139-cli-variables.cmgscript --var user=Ada
cmg run demo-scripts\140-runner-variables.cmgscript --env tenant=demo
```

Use runner declaration variables when the value belongs with the suite or test:

```text
describe "tenant flow" var.tenant=demo {
  test "uses default tenant" {
    expect (${tenant} == "demo")
  }

  test "overrides tenant" var.tenant=staging {
    expect (${tenant} == "staging")
  }
}
```

Declaration variables are inserted before macros and hooks, so helper macros can read them. Test-level values override suite-level values, and explicit `set` actions can still change values later in the current script scope.

## Base URL

Use `--base-url` when scripts should navigate relative to an app root:

```powershell
cmg browser control script --file demo-scripts\141-base-url.cmgscript --base-url https://example.test/app/
cmg run demo-scripts\142-base-url-runner.cmgscript --base-url https://example.test/app/
```

Runner suites and tests can declare their own base URL:

```text
describe "app" baseUrl="https://example.test/app/" {
  test "opens profile" {
    navigate "profile"
  }
}
```

Base URL resolution affects `navigate`, `goto`, `visit`, `openTab`, and `newContext url=`. It does not move the virtual pointer by itself; pointer-aware actions after navigation keep their normal GIF behavior.

## Failure Feedback

Assertions and explicit failures explain what broke. Use `expect` or `assert` for generic conditions, `softExpect` when later diagnostics should still run, `skip` when the flow is not applicable, and `fail` when a script has its own reason to abort:

```text
expect (${savedName} != "") message="Profile name was not saved"
softExpect evaluate "window.optionalPanelReady" == "true" message="Optional panel did not load"

if (${featureEnabled} == false) {
  skip "Feature flag disabled"
}

try {
  fail "Expected optional panel"
} catch error {
  caption "${error}"
}
```

Runner failures include `STEP FAIL` diagnostics on stderr and report entries with the line, action, and reason.

## Scoped Timeouts

Use scoped timeout blocks when one slow section needs longer waits without changing the rest of the script:

```text
withTimeout 10000 {
  waitForSelector "#slow-panel"
}

withTimeout default=5000 navigation=15000 assertion=2000 {
  navigate "C:\Projects\CMG\index.html" waitUntil=load
  expectText "#status" "Ready"
}
```

The old timeout defaults are restored after the block, even if a child action fails and a surrounding `try` catches it. The block itself is non-visual; pointer-aware child actions still record through CMG's normal virtual pointer path.

## Parameterized Tests

Use `test.each`, `it.each`, or `specify.each` when one runner test should execute once per data row:

```text
test.each "opens ${page}" as=page values="profile,checkout" tag=smoke {
  click "#${page}"
}

test.each "opens ${case.name}" as=case json="[{\"name\":\"Profile\",\"selector\":\"#profile\"}]" {
  click "${case.selector}"
}
```

Expanded tests are ordinary scheduled tests. They work with `--list`, `--grep`, `--tag`, retries, sharding, reports, traces, and per-test GIF recording.

## Report Annotations

Use runner declaration metadata when a report should explain ownership, issues, links, requirements, or notes:

```text
describe "checkout" owner=qa annotation.requirement="REQ-1" {
  test "submits payment" issue="BUG-7" link="https://example.test/BUG-7" {
    click "#pay"
  }
}
```

Annotations are report-only metadata. They appear in JSON, HTML, and JUnit reports and do not change browser execution or GIF recording.

## Controlled Inputs That Remount

Framework-controlled fields can replace their DOM element while handling an input event. Keep using the user-facing locator; CMG reacquires the replacement during the same action:

```text
gif "edit profile" {
  waitForElement "getByLabel=Display name"
  fill "getByLabel=Display name" "CMG"
  expectValue "getByLabel=Display name" "CMG"
  click "getByRole=button|Save"
}
```

`fill` uses the native input value setter and a bubbling `InputEvent`, including during progressive GIF typing. See `demo-scripts/159-controlled-input-remount.cmgscript` for a standalone remounting fixture and click-count assertion.

## Automatic GIF Narration

Enable automatic captions at recording scope, then override individual actions only when useful:

```text
gif "guided form" autoCaptions=true captionPosition=auto captionStyle=teaching {
  fill "getByLabel=Display name" "CMG"
  click "getByRole=button|Save" captionTemplate="{action}: {selector}"
  expectText "#status" "Saved"
}
```

CMG does not place entered text in default `fill` or `type` captions. Automatic captions and target-aware placement are skipped entirely when no GIF recorder is active.

## GIF Title Cards

Bookend a recording and add an explicit chapter marker where the story changes:

```text
gif "release" intro="Release verification" outro="Ready to share" {
  click "#deploy"
  intro "Health checks" duration=700
  expectText "#status" "Healthy"
}
```

Title cards hide the virtual pointer. Without active recording, explicit title-card actions skip without injecting page UI.

## Hide And Re-Time GIF Sections

```text
gif "review flow" {
  hideFromGif {
    click "#prepare"
    waitForText "#status" "Prepared"
  }
  speedUpGif factor=3 { pauseGif 900 }
  slowDownGif factor=2 { click "#confirm" }
}
```

Hidden actions still change the page; the next visible frame shows their result. Playback factors affect only encoded frame delays. Both block types are inert wrappers in non-GIF runs, so their children execute without pointer injection.

## Encoder Color Evidence

Retain the browser's exact PNG frames when tuning GIF palette behavior:

```text
recording quality=archival dither=atkinson palette=local colors=256 keepFrames=true {
  gif "brand palette" output="demo-output/brand-palette.gif" {
    click "#show-gradient"
  }
}
```

Then measure a corresponding frame without opening a browser:

```powershell
cmg gif color-diff demo-output\brand-palette.frames\frame-0001.png demo-output\brand-palette.gif --frame 1
```

See `demo-scripts/165-gif-color-controls.cmgscript` and `166-gif-color-controls-runner.cmgscript` for both script forms.

Apply the same controls to a whole direct run:

```powershell
cmg browser control script --file demo-scripts\167-command-gif-color-controls.cmgscript --gif demo-output\command-colors.gif --gif-quality archival --gif-dither atkinson --gif-palette local --gif-colors 192 --keep-frames demo-output\command-colors.frames
```

For runner GIFs, the retained-frame directory is automatically split by artifact name:

```powershell
cmg run demo-scripts\168-command-gif-color-controls-runner.cmgscript --gif demo-output\runner-colors --gif-quality archival --gif-dither sierra --gif-palette adaptive --gif-colors 192 --keep-frames demo-output\runner-color-frames
```

## Focused GIF Framing

Crop to a stable container, retain some context, and downscale only after pointer and caption compositing:

```text
gif "focused checkout" crop="#checkout" cropPadding=24 scale=0.75 maxWidth=700 {
  click "#submit"
  expectText "#status" "Complete"
}
```

The same defaults work for command-level runner artifacts:

```powershell
cmg run demo-scripts\170-command-gif-framing-runner.cmgscript --gif demo-output\framed-runner --gif-crop "#stage" --gif-crop-padding 24 --gif-scale 0.75 --gif-max-width 500
```

See `demo-scripts/169-gif-framing.cmgscript` for block-level framing and demo 170 for whole-run structured tests.

## Narrated Success And Failure Evidence

```text
gif "guided check" captionDuration=450 fadeIn=180 fadeOut=180 {
  narrate "Submit the form" {
    click "#submit"
  }
  expectText "#status" "Complete"
}
```

Successful assertions receive an automatic QA caption. Failed actions receive an error caption before CMG finalizes the partial GIF. Use `assertionCaptions=false` or `failureCaptions=false` when those values should not appear in visual evidence.

See `demo-scripts/171-gif-narration.cmgscript`. Demo 172 intentionally fails:

```powershell
cmg run demo-scripts\172-gif-failure-narration-runner.cmgscript --gif demo-output\failure-narration --gif-hold-on-failure 1400
```

## Accessibility Evidence

Record keyboard labels, the actual focus target, control role/name evidence, and targeted-control contrast warnings without exposing text-entry values:

```text
gif "accessible form" accessibilityEvidence=true {
  press "Tab"
  fill "#name" "Ada"
  click "#submit"
}
```

Use `showKeystrokes { ... }` inside a command-level GIF when only keyboard labels are needed. See `demo-scripts/178-gif-accessibility-evidence.cmgscript` and `demo-scripts/179-gif-accessibility-evidence-runner.cmgscript`.

Use scalable captions and override an inherited warning on a deliberate low-contrast state:

```text
gif "accessibility review" accessibilityEvidence=true captionSize=x-large {
  caption "Review the primary action"
  click "#continue"
  click "#disabled-preview" contrastWarnings=false captionSize=large
}
```

For a whole run, use `--gif-accessibility --caption-size large`. See demos 184 and 185.

Use reduced motion and a high-contrast pointer together when motion itself would make evidence harder to review:

```text
gif "clear evidence" reducedMotion=true highContrastPointer=true {
  click "#continue"
  click "#details" pointerDuration=200
}
```

See demos 180 and 181 for block-level and whole-run CLI usage.

## Event Captions

Summarize browser and network outcomes without exposing their payloads:

```text
gif "event review" eventCaptions=true {
  waitForResponse "/api/checkout"
  waitForDialog "Order complete"
  uploadFiles "#receipt" "demo-scripts/fixtures/event-evidence.txt"
  waitForConsole "receipt stored" consoleCaptions=false
}
```

Use `--gif-event-captions` for a whole direct script or runner test. See demos 186 and 187.

Use scoped aliases when only one section needs evidence:

```text
showMouseButtons {
  mouseDown x=80 y=120
  mouseUp x=80 y=120
}
showNetworkActivity { waitForResponse "/api/save" }
showConsoleActivity { waitForConsole "sync complete" }
```

These blocks execute normally without recording and add no visual DOM, screenshots, or virtual pointer. See demos 214 and 215.

## Outcome Cards

```text
gif "release check" intro="Release verification" resultOutro=true {
  click "#deploy"
  expectText "#status" "Live"
}
```

Use explicit `outro="Approved for release"` when authored text should replace the generated result. Whole-run test GIFs can use `--gif-intro "Release verification" --gif-result-outro`. See demos 188 and 189.

## Efficient Long Recordings

```text
gif "long workflow" sampleEvery=3 {
  hover "#advanced" pointerDuration=1200
  click "#confirm" sampleEvery=1
  pauseGif 800
  pauseGif 800
}
```

Exact duplicate holds coalesce automatically without shortening the timeline. Use `coalesceDuplicates=false` only when inspecting each source capture. Whole-run equivalents are `--gif-sample-every 3` and `--gif-no-coalesce`. See demos 190 and 191.

## Precise Pointer Evidence

```text
gif "precise controls" pointerIdleThreshold=800 {
  click "#tiny-action"
  fill "#search" "CMG evidence"
  moveMouse right pointerSpeed=instant
  pauseGif 1500
  mouseDown selector="#hold" edge=center mouseDownHold=700
  mouseUp selector="#hold" edge=center
}
```

Adaptive contrast, tiny-target callouts, focus pulses, idle halos, teleport markers, and a real post-dispatch pressed state are enabled by default. Override them at recording, block, parent complex-action, or child-action scope. Whole-run CLI options are documented in the script and runner command pages. See demos 192 and 193.

## Color Fidelity Modes

Use `background=` for transparent UI, `gradientMode=smooth` for blended surfaces, and `gradientMode=text` for sharp UI edges. `highContrastPalette=true` is an accessibility-review transform, not a source-fidelity setting. These options inherit through `recording` and can be overridden by a nested GIF block. See demos 194 and 195.

```text
recording background="#f8fafc" gradientMode=smooth {
  gif "color evidence" output="demo-output/color-fidelity.gif" keepFrames=true {
    click "#confirm"
  }
}
```

```powershell
cmg run demo-scripts\195-gif-color-fidelity-runner.cmgscript --gif demo-output\color-runner --gif-background "#f8fafc" --gif-gradient-mode smooth
```

Use `viewport=390x844 pixelRatio=2` for deterministic high-DPI mobile evidence without permanently changing browser state. See demos 196 and 197.

Nested visual narratives can retain their parent context and explain control flow:

```text
recording persistentStepTitle=true sourceLineCaptions=true debugNarration=true captionFormat=markdown {
  step "**Checkout**" {
    step "Run `payment` macro" { call "payment" }
  }
}
```

See demos 198 and 199. Without an active GIF, these recording defaults create no caption, pointer, or screenshot state.

Long visual waits can stay honest without making reviewers watch every millisecond:

```text
recording compressLongWaits=true longWaitThreshold=2000 longWaitDuration=1200 waitProgress=true {
  pauseGif 5000
}
```

See demos 200 and 201. Use `cmg gif trim` afterward when an existing artifact needs precise frame/time editing.

## Preview And Change Recording Settings

```text
setRecording quality=highest pointerSpeed=fast
recordingDefaults captionStyle=qa {
  gif "settings" output="demo-output/settings.gif" {
    click "#first"
    setRecording pointerDuration=900 clickPulse=ripple
    previewRecordingSettings
    click "#second"
  }
}
```

`setRecording` affects subsequent actions only in the current scope. `recordingDefaults` restores its parent settings after the block. Both remain pointer-free without an active recording. Preview a file without Chrome using `cmg browser control script --file <path> --preview-gif-settings`. See demos 202 and 203.

## Annotate Pointer, Target, And State

```text
gif "review" {
  pointerStyle pointerTheme=hand pointerColor="#dc2626"
  annotateTarget "#save" "Primary action"
  click "#save"
  recordVariable "status" label="Current state"
}
```

The pointer style applies to later actions in the current scope. Target annotation uses the same resolved locator as the virtual pointer, and `recordVariable` masks secret-like names by default. See demos 206 and 207.

## Keep Only Useful Focused GIFs

```text
gifIfChanged "save" {
  click "#save"
  gifSnapshot "saved state" duration=600
}

gifOnFailure "save failure" {
  assertText "#status" "Saved"
}
```

The first block compares recorder-free baseline/final page pixels; the second discards passing evidence. Failures always preserve partial choreography. See demos 208 and 209.

## Suite And Test GIF Defaults

```text
describe "visual evidence" gifQuality=medium gifPointerSpeed=fast gifFps=20 {
  test "inherits defaults" { click "#save" }
  test "overrides quality" gifQuality=highest { click "#save" }
}
```

Run with `cmg run <file> -gif <directory>`. Test options override suite values one property at a time. See direct scoped-default demo 210 and runner declaration demo 211.

## Retain Only Useful Runner Evidence

```text
describe "CI evidence" gif=onFailure {
  test "retry evidence" gif=onRetry { assertText "#status" "Complete" }
  test "sampled" gifSampleRate=10 { click "#save" }
  test "temporary passing evidence" gif=always gifCleanPassed=true { click "#publish" }
}
```

Use these declarations with `cmg run <file> -gif <directory>`. Retention is decided after retries; passing cleanup happens after reports and traces are written. Explicit focused recording blocks are unaffected. See demo 212.

For a coarse CI default without declarations:

```powershell
cmg run tests --gif artifacts/gifs --gif-on-retry --retries 2
cmg run tests --gif artifacts/gifs --gif-retention onFailure --gif-sample-rate 10 --report-json artifacts/report.json
```

Use `--gif-clean-passed` only when the report is the durable record and passing files should be removed afterward. See demo 213.

## Check GIF Authoring Before Running

```powershell
cmg browser control script --file demo-scripts\216-gif-authoring-warnings.cmgscript --preview-gif-settings
cmg run demo-scripts\217-gif-large-suite-warning-runner.cmgscript --list --gif demo-output\large-suite
```

Both commands run without a browser. The first identifies GIF-producing provider aliases and focused recording blocks with more than 20 descendant actions. The second suggests failure, retry, sampling, or cleanup retention when a command-level GIF would retain every test in a large selection. The warnings are advisory and preserve exit code `0` for otherwise valid input.

## Disable Recording For Sensitive Runs

```powershell
cmg browser control script --file demo-scripts\218-gif-recording-disabled.cmgscript --gif demo-output\218-whole-run.gif --no-gif
$env:CMG_DISABLE_GIF = "1"
cmg run tests --gif artifacts\gifs
```

The kill switch suppresses whole-run and nested recording, but actions inside recording blocks still execute. No screenshots, recording overlays, or virtual pointer are created. Unset `CMG_DISABLE_GIF` after the sensitive CI step when later commands should record normally.

## Share GIF Defaults Across Projects

```powershell
cmg browser --port 9467 launch --headless --idle-timeout 20m
cmg run demo-scripts\219-gif-project-settings-runner.cmgscript --config demo-scripts\gif-settings.example.json --project chrome-visual --browser-port 9467 --gif-scale 0.75 --caption-style qa
cmg browser --port 9467 close
```

`gifSettings` merges root, selected project, and explicit CLI properties independently. The demo inherits root crop/viewport settings, project quality/timing/padding, and CLI scale/caption style. Suite, test, recording block, and action overrides remain more specific.

## Narrate A Whole Run Automatically

```powershell
cmg browser control script --file demo-scripts\220-gif-auto-captions-cli.cmgscript --gif demo-output\220-auto-captions.gif --auto-captions --caption-style teaching --caption-position bottom --caption-template "{step}: {action} {selector}"
cmg run demo-scripts\221-gif-auto-captions-cli-runner.cmgscript --gif demo-output\221-auto-captions --auto-captions --caption-style qa --caption-position bottom --caption-template "{step}: {action} {selector}"
```

`{step}` exposes current lexical step/macro/loop context and `{assertion}` identifies assertion actions without including expected or entered values. Supplying `--caption-template` enables narration even when `--auto-captions` is omitted. Both options are inert without recording.

## Apply Whole-Run Privacy Defaults

```powershell
cmg browser control script --file demo-scripts\222-gif-cli-redaction.cmgscript --gif demo-output\222-redacted.gif --gif-redact "#email" --gif-blur ".token" --gif-auto-redact none --gif-redaction-safety strict
cmg run demo-scripts\223-gif-cli-redaction-runner.cmgscript --gif demo-output\223-redacted --gif-mask "#email" --gif-blur ".token" --gif-auto-redact none --gif-redaction-safety strict
```

Solid and blur rules combine. Repeat an option for multiple selectors. The same defaults can be stored as root/project `gifSettings.redact`, `mask`, and `blur` arrays. Rules are applied only around frame capture and create no page overlays or virtual pointer without recording.

For broad capture safety, `autoRedact=privacy` combines all automatic detectors:

```text
gif "privacy preset" autoRedact=privacy {
  click "#save"
}
```

See demo 224. Prefer explicit `redact=` selectors when sensitive and public text share one element.

## Stabilize Moving And Obstructed Targets

```text
gif "framing resilience" safeArea=36 layoutStability=500 {
  click "#save"
}
```

The default `safeArea=24` keeps pointer evidence away from viewport/crop edges and detected sticky or fixed blockers. The default `layoutStability=150` waits for two stable animation frames before CMG fixes pointer coordinates. Set either to `0` for exact-edge or immediate-coordinate evidence. Runner suites use `gifSafeArea=` and `gifLayoutStability=`; whole runs use `--gif-safe-area` and `--gif-layout-stability`. See demos 225 and 226.

## Keep Target Labels Readable During Pointer Travel

```text
gif "approval" pointerPath=auto pointerDuration=1400 {
  click "#approve"
}
```

`auto` is the default. For large elements, CMG uses the resolved target rectangle to travel toward the nearest outside edge, then enters only for the final center click. This preserves pointer accuracy without covering the label throughout the journey. Use `direct`, `arc`, or `manhattan` for authored routes; `avoid-target` forces the target-aware approach. Whole runs use `--pointer-path` and `--drag-path`; suites use `gifPointerPath` and `gifDragPath`. See demos 227 and 228.

## GIF Diagnostics

Use a frame-only HUD and machine-readable sidecar when pointer or selector choreography needs investigation:

```text
gif "debug checkout" debug=true debugScroll=false {
  step "Submit" {
    click "#submit"
  }
}
```

CMG writes `<gif-name>.debug.json` and emits `GIF_DEBUG <path>`. See demos 182 and 183.

For target-quality diagnostics, run demo 204 with command-level `--gif`, or demo 205 with runner `-gif`. Active recording warns about ambiguous selectors, tiny targets, automatic offscreen scrolling, and visual options on non-visual actions. The same scripts without GIF recording do not run recorder probes or inject the virtual pointer.

## Common Next Steps

| Need | Where To Go |
| --- | --- |
| Choose direct scripts or `cmg run` | [Script vs Runner](script-vs-runner.md) |
| Find an action family quickly | [Action Index](action-index.md) |
| Learn syntax, logic, loops, and macros | [Syntax](syntax.md) |
| Understand GIF blocks and `--gif` | [GIF Recording](gif-recording.md) |
| Write maintainable scripts | [Style Guide](style-guide.md) |
| Browse every example pattern | [Cookbook Reference](cookbook-reference.md) |
