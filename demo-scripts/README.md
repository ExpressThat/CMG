# CMG Demo Scripts

These `.cmgscript` files demonstrate the browser scripting language against the repository's `index.html` test page.

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

Record the GIF-only `moveMouse` demo:

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

Run the structured runner demo with reports:

```powershell
dotnet run -- run demo-scripts\20-v2-runner-flow.cmgscript --report-json demo-output\runner.json --report-html demo-output\runner.html
```

Record the whole runner demo test as a GIF:

```powershell
dotnet run -- run demo-scripts\20-v2-runner-flow.cmgscript --gif demo-output\runner-gifs
```

Close the browser:

```powershell
dotnet run -- browser close
dotnet run -- --edge browser close
dotnet run -- --firefox browser close
```

## Scripts

- `01-dialog-flow.cmgscript`: Opens the profile dialog, types into fields, captures the dialog, and closes it.
- `02-validation-flow.cmgscript`: Exercises custom validation messages on the test page.
- `03-drag-drop-flow.cmgscript`: Drags command tiles into the queue and asserts the result.
- `04-capture-elements.cmgscript`: Captures HTML, element screenshots, and a full page screenshot.
- `05-variables-and-evaluate.cmgscript`: Demonstrates variables, assertions, JavaScript evaluation, and viewport sizing.
- `06-tabs-and-keys.cmgscript`: Demonstrates tab listing, tab activation, keyboard input, shortcut chords, and dialog dismissal.
- `07-complex-drag-flow.cmgscript`: Demonstrates block `dragAndDrop` with delay, hover, and drop steps.
- `08-gif-move-mouse.cmgscript`: Demonstrates GIF-only `moveMouse` and visible pointer movement.
- `09-drag-autoscroll.cmgscript`: Demonstrates `moveMouse "bottom"` inside a GIF drag block.
- `10-css-hover-states.cmgscript`: Demonstrates real CSS `:hover` states during GIF pointer movement.
- `13-rich-locators.cmgscript`: Demonstrates direct browser-control text and label locators.
- `14-pointer-click-variants.cmgscript`: Demonstrates `doubleClick` and `contextClick` with pointer movement.
- `15-popup-alias.cmgscript`: Demonstrates `waitForPopup` as a popup-named tab wait.
- `16-generic-event-waits.cmgscript`: Demonstrates provider-style `waitForEvent` aliases.
- `17-http-credentials.cmgscript`: Demonstrates page-side HTTP credential automation for fetch/XHR.
- `18-expose-function.cmgscript`: Demonstrates page-side exposed functions.
- `19-direct-gif-block.cmgscript`: Demonstrates direct browser-control `gif` blocks.
- `20-v2-runner-flow.cmgscript`: Demonstrates the structured runner DSL, `step`, and `gif` blocks.
- `21-websocket-routing.cmgscript`: Demonstrates page-side WebSocket routing, waits, and match modes.
- `22-browser-environment.cmgscript`: Demonstrates page-side browser environment controls.
- `23-record-video-alias.cmgscript`: Demonstrates `recordVideo` as a CMG GIF recording alias.
- `24-touch-clipboard.cmgscript`: Demonstrates touch-style `tap` and deterministic page-side clipboard actions.
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
- `41-locator-filters.cmgscript`: Demonstrates `first=`, `nth=`, `has=`, `hasNot=`, `hasText=`, `hasNotText=`, and `visible=` locator filters with pointer-aware actions.
- `42-locator-filters-runner.cmgscript`: Demonstrates the same locator filters in the structured runner DSL.
- `43-scroll-wheel.cmgscript`: Demonstrates window and element `scrollTo`, `scrollBy`, and wheel input in direct scripts.
- `44-scroll-wheel-runner.cmgscript`: Demonstrates the same scroll and wheel actions in the structured runner DSL.
- `45-provider-aliases.cmgscript`: Demonstrates provider-style aliases such as `toHaveTitle`, `toContainText`, `pressSequentially`, `setInputFiles`, and `dragTo`.
- `46-provider-aliases-runner.cmgscript`: Demonstrates the same provider-style aliases in the structured runner DSL.
- `47-provider-structure-runner.cmgscript`: Demonstrates `describe`, `it`, `specify`, `before`, and `after` runner aliases.
- `48-weird-formatting.cmgscript`: Demonstrates direct-script inline blocks, quoted braces, semicolon separators, and repeated spacing.
- `49-weird-formatting-runner.cmgscript`: Demonstrates runner inline blocks, quoted braces, semicolon separators, and repeated spacing.
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
- `99-failure-missing-element.cmgscript`: Intentional failure example for error handling.

Generated screenshots are written to `demo-output/`.
