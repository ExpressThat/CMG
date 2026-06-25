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
- `06-tabs-and-keys.cmgscript`: Demonstrates tab listing, tab activation, keyboard input, and dialog dismissal.
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
- `21-websocket-routing.cmgscript`: Demonstrates page-side WebSocket routing and waits.
- `22-browser-environment.cmgscript`: Demonstrates page-side browser environment controls.
- `23-record-video-alias.cmgscript`: Demonstrates `recordVideo` as a CMG GIF recording alias.
- `24-touch-clipboard.cmgscript`: Demonstrates touch-style `tap` and deterministic page-side clipboard actions.
- `25-provider-navigation-aliases.cmgscript`: Demonstrates `visit` and `goto` navigation aliases.
- `26-provider-text-assertions.cmgscript`: Demonstrates `contains`, `containsText`, and `waitForText` aliases.
- `27-selector-evaluation.cmgscript`: Demonstrates selector-scoped evaluation and `set` block capture.
- `28-element-getters.cmgscript`: Demonstrates element getter output and `set` block capture.
- `29-filtered-network-waits.cmgscript`: Demonstrates method/status/body filters for network waits.
- `30-control-flow-macros.cmgscript`: Demonstrates imports, macros, nested scoped helpers, conditionals, and selector iteration.
- `30-shared-macros.cmgscript`: Imported helper macros for the control-flow demo.
- `31-control-flow-runner.cmgscript`: Demonstrates the same control-flow and macro features in the structured runner DSL.
- `32-loop-control.cmgscript`: Demonstrates `repeat`, bounded `while`, `break`, and `continue`.
- `33-try-catch-finally.cmgscript`: Demonstrates recoverable failure handling with `try`, `catch`, and `finally`.
- `34-macro-scoping.cmgscript`: Demonstrates macro parent-scope lookup and local variable shadowing in direct scripts.
- `35-macro-scoping-runner.cmgscript`: Demonstrates macro parent-scope lookup and local variable shadowing in the structured runner DSL.
- `36-runner-selection.cmgscript`: Demonstrates runner `only=true` focus and `skip=true` metadata.
- `37-evaluated-assertions.cmgscript`: Demonstrates `expectEval`, `assertEval`, and expression matchers.
- `38-before-after-all.cmgscript`: Demonstrates root and suite `beforeAll` / `afterAll` hooks.
- `99-failure-missing-element.cmgscript`: Intentional failure example for error handling.

Generated screenshots are written to `demo-output/`.
