# CMG Demo Scripts

These `.cmgscript` files demonstrate CMG against the repository's `index.html` test page. Start with a short path, then use the full catalogue when you need a specific capability.

## Start Here

If you are learning CMG, run these five demos first:

| Goal | Demo |
| --- | --- |
| Make a first browser-control GIF | `01-dialog-flow.cmgscript` |
| See pointer-accurate hover behavior | `10-css-hover-states.cmgscript` |
| Watch drag ghosts and pointer movement | `07-complex-drag-flow.cmgscript` |
| Run a structured test | `20-runner-flow.cmgscript` |
| Learn variables, loops, and macros | `30-control-flow-macros.cmgscript` |

After that, use the learning groups below before treating the full script list as a reference.

## Learning Groups

| Need | Demos |
| --- | --- |
| Browser-control basics | `01-dialog-flow.cmgscript`, `02-validation-flow.cmgscript`, `05-variables-and-evaluate.cmgscript`, `141-base-url.cmgscript` |
| Visual evidence | `07-complex-drag-flow.cmgscript`, `08-gif-move-mouse.cmgscript`, `09-drag-autoscroll.cmgscript`, `10-css-hover-states.cmgscript`, `148-gif-quality.cmgscript`, `149-gif-pointer-choreography.cmgscript`, `150-gif-failure-hold.cmgscript`, `151-gif-timeline.cmgscript`, `152-runner-gif-report-metadata.cmgscript`, `153-recording-scope.cmgscript`, `154-runner-recording-scope.cmgscript` |
| Structured tests | `20-runner-flow.cmgscript`, `36-runner-selection.cmgscript`, `38-before-after-all.cmgscript` |
| Control flow and reuse | `30-control-flow-macros.cmgscript`, `32-loop-control.cmgscript`, `33-try-catch-finally.cmgscript`, `34-macro-scoping.cmgscript` |
| Assertions and failure feedback | `52-explicit-fail.cmgscript`, `126-generic-expect.cmgscript`, `128-soft-expect.cmgscript`, `132-runtime-skip.cmgscript` |
| Runner reporting and data | `134-scoped-timeouts.cmgscript`, `136-parameterized-tests.cmgscript`, `137-parameterized-json-tests.cmgscript`, `138-report-annotations.cmgscript`, `140-runner-variables.cmgscript`, `142-base-url-runner.cmgscript` |

For a guided introduction, use the [Quick Start](../docs/quick-start.md), [examples guide](../docs/scripting/examples.md), and [style guide](../docs/scripting/style-guide.md).

## Run A Demo

Start the CMG-controlled browser:

```powershell
dotnet run -- browser launch
```

Use Chrome explicitly, Edge, or Firefox:

```powershell
dotnet run -- --chrome browser launch
dotnet run -- --edge browser launch
dotnet run -- --firefox browser launch
```

Run a script:

```powershell
dotnet run -- browser control script --file demo-scripts\01-dialog-flow.cmgscript
dotnet run -- browser control script --file demo-scripts\139-cli-variables.cmgscript --var user=Ada
dotnet run -- browser control script --file demo-scripts\141-base-url.cmgscript --base-url https://example.test/app/
```

Validate a script without connecting to a browser:

```powershell
dotnet run -- browser control validateScript --file demo-scripts\48-weird-formatting.cmgscript
```

Run a demo script in Edge or Firefox:

```powershell
dotnet run -- --edge browser control script --file demo-scripts\01-dialog-flow.cmgscript
dotnet run -- --firefox browser control script --file demo-scripts\01-dialog-flow.cmgscript
```

Record a script as a GIF:

```powershell
dotnet run -- browser control script --file demo-scripts\01-dialog-flow.cmgscript --gif demo-output\dialog-flow.gif
```

Record the complex drag demo as a GIF:

```powershell
dotnet run -- browser control script --file demo-scripts\07-complex-drag-flow.cmgscript --gif demo-output\complex-drag.gif
```

Record the recording-only `moveMouse` demo:

```powershell
dotnet run -- browser control script --file demo-scripts\08-gif-move-mouse.cmgscript --gif demo-output\gif-move-mouse.gif
```

Record the drag autoscroll pattern as a GIF:

```powershell
dotnet run -- browser control script --file demo-scripts\09-drag-autoscroll.cmgscript --gif demo-output\drag-autoscroll.gif
```

Record the CSS hover demo as a GIF:

```powershell
dotnet run -- browser control script --file demo-scripts\10-css-hover-states.cmgscript --gif demo-output\css-hover-states.gif
```

Record the pointer choreography demo:

```powershell
dotnet run -- browser control script --file demo-scripts\149-gif-pointer-choreography.cmgscript --gif demo-output\pointer-choreography.gif --pointer-duration 500
dotnet run -- browser control script --file demo-scripts\150-gif-failure-hold.cmgscript --gif demo-output\failure-hold.gif --gif-hold-on-failure 1800 --gif-timeline demo-output\timelines
dotnet run -- browser control script --file demo-scripts\151-gif-timeline.cmgscript
dotnet run -- browser control script --file demo-scripts\153-recording-scope.cmgscript
dotnet run -- run demo-scripts\154-runner-recording-scope.cmgscript --gif demo-output\runner-gifs
dotnet run -- gif inspect demo-output\timeline-evidence.gif
dotnet run -- gif compare demo-output\timeline-evidence.gif demo-output\pointer-choreography.gif
dotnet run -- gif storyboard demo-output\timeline-evidence.gif --output demo-output\timeline-storyboard.png --columns 4 --max-frames 12
dotnet run -- gif optimize demo-output\timeline-evidence.gif --output demo-output\timeline-evidence.optimized.gif
dotnet run -- gif presets
```

Run the structured runner demo with reports:

```powershell
dotnet run -- run demo-scripts\20-runner-flow.cmgscript --gif demo-output\runner-gifs --report-json demo-output\runner.json --report-html demo-output\runner.html
dotnet run -- run demo-scripts\152-runner-gif-report-metadata.cmgscript --gif demo-output\runner-gifs --gif-timeline demo-output\timelines --gif-warn-size 500KB --gif-max-size 2MB --gif-max-duration 10s --report-json demo-output\runner-gif-metadata.json --report-html demo-output\runner-gif-metadata.html --report-junit demo-output\runner-gif-metadata.xml
```

The second command can also emit `GIF_WARN_PALETTE` when the recorded page uses enough colors to put pressure on GIF palette fidelity, and can fail with `GIF_MAX_SIZE` or `GIF_MAX_DURATION` if a GIF exceeds the configured review budget.

Record the whole runner demo test as a GIF:

```powershell
dotnet run -- run demo-scripts\20-runner-flow.cmgscript --gif demo-output\runner-gifs
```

Close the browser:

```powershell
dotnet run -- browser close
dotnet run -- --edge browser close
dotnet run -- --firefox browser close
```

## Full Catalogue

- `01-dialog-flow.cmgscript`: Opens the profile dialog, types into fields, captures the dialog, and closes it.
- `02-validation-flow.cmgscript`: Exercises custom validation messages on the test page.
- `03-drag-drop-flow.cmgscript`: Drags command tiles into the queue and asserts the result.
- `04-capture-elements.cmgscript`: Captures HTML, element screenshots, and a full page screenshot.
- `05-variables-and-evaluate.cmgscript`: Demonstrates variables, assertions, JavaScript evaluation, and viewport sizing.
- `06-tabs-and-keys.cmgscript`: Demonstrates tab listing, tab activation, keyboard input, shortcut chords, and dialog dismissal.
- `07-complex-drag-flow.cmgscript`: Demonstrates block `dragAndDrop` with delay, hover, and drop steps.
- `08-gif-move-mouse.cmgscript`: Demonstrates recording-only `moveMouse` and visible pointer movement.
- `09-drag-autoscroll.cmgscript`: Demonstrates `moveMouse "bottom"` inside a GIF drag block.
- `10-css-hover-states.cmgscript`: Demonstrates real CSS `:hover` states during GIF pointer movement.
- `13-rich-locators.cmgscript`: Demonstrates direct browser-control text and label locators.
- `14-pointer-click-variants.cmgscript`: Demonstrates `doubleClick` and `contextClick` with pointer movement, modifiers, and offsets.
- `15-popup-alias.cmgscript`: Demonstrates `waitForPopup` as a popup-named tab wait.
- `16-generic-event-waits.cmgscript`: Demonstrates provider-style `waitForEvent` aliases.
- `17-http-credentials.cmgscript`: Demonstrates page-side HTTP credential automation for fetch/XHR.
- `18-expose-function.cmgscript`: Demonstrates page-side exposed functions.
- `19-direct-gif-block.cmgscript`: Demonstrates direct browser-control `gif` blocks.
- `20-runner-flow.cmgscript`: Demonstrates the structured runner DSL, `step`, and `gif` blocks.
- `21-websocket-routing.cmgscript`: Demonstrates page-side WebSocket routing, waits, and match modes.
- `22-browser-environment.cmgscript`: Demonstrates page-side browser environment controls.
- `23-record-video-alias.cmgscript`: Demonstrates `recordVideo` as a CMG GIF recording alias.
- `24-touch-clipboard.cmgscript`: Demonstrates selector and coordinate touch-style `tap` plus deterministic page-side clipboard actions.
- `25-provider-navigation-aliases.cmgscript`: Demonstrates `visit` and `goto` navigation aliases.
- `26-provider-text-assertions.cmgscript`: Demonstrates positive and negative text assertion aliases.
- `27-selector-evaluation.cmgscript`: Demonstrates selector-scoped evaluation and `set` block capture.
- `28-element-getters.cmgscript`: Demonstrates element getter output and `set` block capture.
- `29-filtered-network-waits.cmgscript`: Demonstrates method/status/body filters for network waits.
- `30-control-flow-macros.cmgscript`: Demonstrates imports, macros, nested scoped helpers, conditionals, and selector iteration.
- `30-shared-macros.cmgscript`: Imported helper macros for the control-flow demo.
- `31-control-flow-runner.cmgscript`: Demonstrates the same control-flow and macro features in the structured runner DSL.
- `32-loop-control.cmgscript`: Demonstrates `repeat`, bounded `while`/`until`, post-condition loops, `break`, and `continue`.
- `33-try-catch-finally.cmgscript`: Demonstrates recoverable failure handling with `try`, `catch`, and `finally`.
- `34-macro-scoping.cmgscript`: Demonstrates macro parent-scope lookup and local variable shadowing in direct scripts.
- `35-macro-scoping-runner.cmgscript`: Demonstrates macro parent-scope lookup and local variable shadowing in the structured runner DSL.
- `36-runner-selection.cmgscript`: Demonstrates runner `only=true` focus and `skip=true` metadata.
- `37-evaluated-assertions.cmgscript`: Demonstrates `expectEval`, `assertEval`, and expression matchers.
- `38-before-after-all.cmgscript`: Demonstrates root and suite `beforeAll` / `afterAll` hooks.
- `39-switch-control.cmgscript`: Demonstrates direct-script `switch`, `case`, `default`, and shared word comparison operators.
- `40-switch-control-runner.cmgscript`: Demonstrates the same switch control flow in the structured runner DSL.
- `41-locator-filters.cmgscript`: Demonstrates exact text, regex text, role/name, `first=`, `nth=`, `has=`, `hasNot=`, `hasText=`, `hasNotText=`, `visible=`, `or=`, `and=`, `strict=`, `inside=`, `closest=`, `parent=`, `next=`, and `previous=` locators with pointer-aware actions.
- `42-locator-filters-runner.cmgscript`: Demonstrates the same locator filters in the structured runner DSL.
- `90-shadow-locators.cmgscript`: Demonstrates `shadow=` and `shadowText=` locators for open shadow roots with pointer-aware actions.
- `91-shadow-locators-runner.cmgscript`: Demonstrates the same open shadow-root locators in the structured runner DSL.
- `43-scroll-wheel.cmgscript`: Demonstrates window and element `scrollTo`, `scrollBy`, and wheel input in direct scripts.
- `44-scroll-wheel-runner.cmgscript`: Demonstrates the same scroll and wheel actions in the structured runner DSL.
- `45-provider-aliases.cmgscript`: Demonstrates provider-style aliases such as `toHaveTitle`, `toContainText`, `pressSequentially`, `setInputFiles`, and `dragTo`.
- `46-provider-aliases-runner.cmgscript`: Demonstrates the same provider-style aliases in the structured runner DSL.
- `47-provider-structure-runner.cmgscript`: Demonstrates `describe`, `it`, `specify`, `before`, and `after` runner aliases.
- `48-weird-formatting.cmgscript`: Demonstrates direct-script inline blocks, quoted braces, semicolon separators, comments, and repeated spacing.
- `49-weird-formatting-runner.cmgscript`: Demonstrates runner inline blocks, quoted braces, semicolon separators, comments, and repeated spacing.
- `50-retry-block.cmgscript`: Demonstrates direct-script `retry` around pointer-aware actions.
- `51-retry-block-runner.cmgscript`: Demonstrates the same `retry` block in the structured runner DSL.
- `52-explicit-fail.cmgscript`: Demonstrates catchable explicit `fail` in a direct script.
- `53-explicit-fail-runner.cmgscript`: Demonstrates catchable explicit `fail` in the structured runner DSL.
- `54-console-absence.cmgscript`: Demonstrates `expectNoConsole` and `toHaveNoConsole` in a direct script.
- `55-console-absence-runner.cmgscript`: Demonstrates the same console absence assertions in the structured runner DSL.
- `56-page-error-absence.cmgscript`: Demonstrates `expectNoPageError` and `toHaveNoPageError` in a direct script.
- `57-page-error-absence-runner.cmgscript`: Demonstrates the same page-error absence assertions in the structured runner DSL.
- `58-to-pass-block.cmgscript`: Demonstrates provider-style `toPass` retrying assertion blocks in a direct script.
- `59-to-pass-block-runner.cmgscript`: Demonstrates the same `toPass` block in the structured runner DSL.
- `60-navigation-match-modes.cmgscript`: Demonstrates exact, regex, and case-insensitive URL/title matching in a direct script.
- `61-navigation-match-modes-runner.cmgscript`: Demonstrates the same navigation match modes in the structured runner DSL.
- `62-network-match-modes.cmgscript`: Demonstrates exact, regex, case-insensitive network URL matching, file-backed mocked bodies, and response headers in a direct script.
- `63-network-match-modes-runner.cmgscript`: Demonstrates the same network matching, file-backed body, and header features in the structured runner DSL.
- `64-worker-intercept.cmgscript`: Demonstrates worker fetch interception with regex matching, file-backed bodies, and headers in a direct script.
- `65-worker-intercept-runner.cmgscript`: Demonstrates the same worker interception features in the structured runner DSL.
- `66-frame-match-modes.cmgscript`: Demonstrates frame-local text assertion match modes in a direct script.
- `67-frame-match-modes-runner.cmgscript`: Demonstrates frame-local text assertion match modes in `cmg run`.
- `68-cookie-attributes.cmgscript`: Demonstrates page-context cookie attributes in a direct script.
- `69-cookie-attributes-runner.cmgscript`: Demonstrates the same cookie attributes in the structured runner DSL.
- `70-device-emulation.cmgscript`: Demonstrates named-device emulation in a direct script.
- `71-device-emulation-runner.cmgscript`: Demonstrates named-device emulation in the structured runner DSL.
- `72-script-tracing.cmgscript`: Demonstrates direct-script partial trace capture.
- `73-script-tracing-runner.cmgscript`: Demonstrates partial trace capture in the structured runner DSL.
- `74-element-inspection.cmgscript`: Demonstrates element count, text array, and bounding box getters in a direct script.
- `75-element-inspection-runner.cmgscript`: Demonstrates element inspection getters in the structured runner DSL.
- `76-network-idle.cmgscript`: Demonstrates provider-style network-idle waits in a direct script.
- `77-network-idle-runner.cmgscript`: Demonstrates provider-style network-idle waits in the structured runner DSL.
- `78-media-emulation.cmgscript`: Demonstrates provider-style media emulation in a direct script.
- `79-media-emulation-runner.cmgscript`: Demonstrates provider-style media emulation in the structured runner DSL.
- `80-reload-wait.cmgscript`: Demonstrates provider-style reload wait options in a direct script.
- `81-reload-wait-runner.cmgscript`: Demonstrates provider-style reload wait options in the structured runner DSL.
- `82-history-wait.cmgscript`: Demonstrates provider-style history navigation wait options in a direct script.
- `83-history-wait-runner.cmgscript`: Demonstrates provider-style history navigation wait options in the structured runner DSL.
- `84-navigation-wait.cmgscript`: Demonstrates provider-style initial navigation wait options in a direct script.
- `85-navigation-wait-runner.cmgscript`: Demonstrates provider-style initial navigation wait options in the structured runner DSL.
- `86-within-scope.cmgscript`: Demonstrates scoped selector actions with `within` in a direct script.
- `87-within-scope-runner.cmgscript`: Demonstrates scoped selector actions with `within` in the structured runner DSL.
- `88-frame-block.cmgscript`: Demonstrates provider-style frame-scoped blocks in a direct script.
- `89-frame-block-runner.cmgscript`: Demonstrates provider-style frame-scoped blocks in the structured runner DSL.
- `90-shadow-locators.cmgscript`: Demonstrates shadow DOM locators in a direct script.
- `91-shadow-locators-runner.cmgscript`: Demonstrates shadow DOM locators in the structured runner DSL.
- `92-negative-assertions.cmgscript`: Demonstrates negative state and unchecked assertions in a direct script.
- `93-negative-assertions-runner.cmgscript`: Demonstrates negative state and unchecked assertions in the structured runner DSL.
- `94-provider-declarations-runner.cmgscript`: Demonstrates provider-style `.skip`, `.fixme`, and `.todo` runner declarations.
- `95-provider-locators.cmgscript`: Demonstrates provider-style `getByText`, `getByRole`, `getByLabel`, `getByTestId`, `getByPlaceholder`, `getByAltText`, and `getByTitle` locators in a direct script.
- `96-provider-locators-runner.cmgscript`: Demonstrates provider-style `getBy*` locators in the structured runner DSL.
- `97-worker-event.cmgscript`: Demonstrates provider-style `waitForEvent worker` in a direct script.
- `98-worker-event-runner.cmgscript`: Demonstrates provider-style `waitForEvent worker` in the structured runner DSL.
- `99-failure-missing-element.cmgscript`: Intentional failure example for error handling.
- `100-click-options.cmgscript`: Demonstrates optioned click dispatch with button, modifiers, click count, and delay in a direct script.
- `101-click-options-runner.cmgscript`: Demonstrates optioned click dispatch in the structured runner DSL.
- `102-typing-delay.cmgscript`: Demonstrates provider-style typing delay in a direct script.
- `103-typing-delay-runner.cmgscript`: Demonstrates provider-style typing delay in the structured runner DSL.
- `104-select-option-targets.cmgscript`: Demonstrates provider-style select option targets in a direct script.
- `105-select-option-targets-runner.cmgscript`: Demonstrates provider-style select option targets in the structured runner DSL.
- `106-hover-options.cmgscript`: Demonstrates optioned hover dispatch in a direct script.
- `107-hover-options-runner.cmgscript`: Demonstrates optioned hover dispatch in the structured runner DSL.
- `108-press-delay.cmgscript`: Demonstrates delayed keyboard press behavior in a direct script.
- `109-press-delay-runner.cmgscript`: Demonstrates delayed keyboard press behavior in the structured runner DSL.
- `110-drag-offsets.cmgscript`: Demonstrates source and target offsets for simple drag in a direct script.
- `111-drag-offsets-runner.cmgscript`: Demonstrates source and target offsets for simple drag in the structured runner DSL.
- `112-direct-step-block.cmgscript`: Demonstrates shared `step` caption blocks in a direct script.
- `113-runner-step-block.cmgscript`: Demonstrates shared `step` caption blocks in the structured runner DSL.
- `114-pointer-click-variants-runner.cmgscript`: Demonstrates optioned double-click and context-click dispatch in the structured runner DSL.
- `115-default-timeouts.cmgscript`: Demonstrates script-level default timeout policy in a direct script.
- `116-default-timeouts-runner.cmgscript`: Demonstrates script-level default timeout policy in the structured runner DSL.
- `117-slow-runner.cmgscript`: Demonstrates provider-style `test.slow`, `describe.slow`, and inherited slow timeout policy.
- `118-screenshot-clip.cmgscript`: Demonstrates clipped page screenshot artifacts in a direct script.
- `119-screenshot-clip-runner.cmgscript`: Demonstrates clipped page screenshot artifacts in the structured runner DSL.
- `120-screenshot-style.cmgscript`: Demonstrates temporary CSS applied only to screenshot artifacts in a direct script.
- `121-screenshot-style-runner.cmgscript`: Demonstrates temporary CSS applied only to screenshot artifacts in the structured runner DSL.
- `122-highlight.cmgscript`: Demonstrates temporary visual element highlighting in a direct script.
- `123-highlight-runner.cmgscript`: Demonstrates temporary visual element highlighting in the structured runner DSL.
- `124-element-style-property.cmgscript`: Demonstrates computed CSS and JavaScript property getters in a direct script.
- `125-element-style-property-runner.cmgscript`: Demonstrates computed CSS and JavaScript property getters in the structured runner DSL.
- `126-generic-expect.cmgscript`: Demonstrates generic `expect` assertions over variables and action output in a direct script.
- `127-generic-expect-runner.cmgscript`: Demonstrates generic `expect` assertions in the structured runner DSL.
- `128-soft-expect.cmgscript`: Demonstrates soft generic assertions that continue direct script execution before failing at the end.
- `129-soft-expect-runner.cmgscript`: Demonstrates soft generic assertions in the structured runner DSL.
- `130-collection-loops.cmgscript`: Demonstrates `foreachJson` and `foreachList` over action output and delimited values.
- `131-collection-loops-runner.cmgscript`: Demonstrates collection loops in the structured runner DSL.
- `132-runtime-skip.cmgscript`: Demonstrates runtime `skip` in a direct script.
- `133-runtime-skip-runner.cmgscript`: Demonstrates runtime `skip` in the structured runner DSL.
- `134-scoped-timeouts.cmgscript`: Demonstrates scoped timeout blocks in a direct script.
- `135-scoped-timeouts-runner.cmgscript`: Demonstrates scoped timeout blocks in the structured runner DSL.
- `136-parameterized-tests.cmgscript`: Demonstrates runner `test.each` with primitive list rows.
- `137-parameterized-json-tests.cmgscript`: Demonstrates runner `test.each` with JSON object rows and dotted variables.
- `138-report-annotations.cmgscript`: Demonstrates suite and test report annotations.
- `139-cli-variables.cmgscript`: Demonstrates command-line `--var` / `--env` values in a direct script.
- `140-runner-variables.cmgscript`: Demonstrates suite and test `var.*` declaration variables.
- `141-base-url.cmgscript`: Demonstrates command-line `--base-url` for relative navigation in a direct script.
- `142-base-url-runner.cmgscript`: Demonstrates runner `baseUrl=` declarations for relative navigation.
- `143-screenshot-mask.cmgscript`: Demonstrates screenshot artifact masks in a direct script.
- `144-screenshot-mask-runner.cmgscript`: Demonstrates screenshot artifact masks in the structured runner DSL.
- `145-screenshot-deterministic.cmgscript`: Demonstrates artifact-only animation and caret stabilization in a direct script.
- `146-screenshot-deterministic-runner.cmgscript`: Demonstrates artifact-only animation and caret stabilization in the structured runner DSL.
- `147-run-config.cmgscript`: Demonstrates `cmg run --config` defaults with variables, selection, project matrices, reports, traces, and retries.
- `148-gif-quality.cmgscript`: Demonstrates GIF quality presets on recording blocks and aliases.
- `149-gif-pointer-choreography.cmgscript`: Demonstrates GIF pointer duration, speed, easing, click pulses, parent block defaults, and child drag overrides.
- `150-gif-failure-hold.cmgscript`: Demonstrates final failure-state holds in partial GIF artifacts.
- `151-gif-timeline.cmgscript`: Demonstrates block-level GIF timeline metadata with `timeline=true`.
- `152-runner-gif-report-metadata.cmgscript`: Demonstrates runner JSON `gifMetadata` entries, HTML GIF previews, and JUnit GIF artifact properties for command-level GIF artifacts.
- `153-recording-scope.cmgscript`: Demonstrates `recording { ... }` scoped GIF defaults, `frameDelay=`, and nested `fps=` inherited by a GIF block.
- `154-runner-recording-scope.cmgscript`: Demonstrates `recording { ... }` scoped GIF defaults during a command-level runner recording.
- `run-config.example.json`: Example JSON config for `cmg run --config` and `--project`.

Generated screenshots are written to `demo-output/`.
