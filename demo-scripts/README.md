# CMG Demo Scripts

These `.cmgscript` files demonstrate the browser scripting language against the repository's `index.html` test page.

## Run A Demo

Start the CMG-controlled browser:

```powershell
dotnet run -- browser launch
```

Use Firefox instead of Chrome:

```powershell
dotnet run -- --firefox browser launch
```

Run a script:

```powershell
dotnet run -- browser control script --file demo-scripts\01-dialog-flow.cmgscript
```

Run a demo script in Firefox:

```powershell
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

Close the browser:

```powershell
dotnet run -- browser close
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
- `99-failure-missing-element.cmgscript`: Intentional failure example for error handling.

Generated screenshots are written to `demo-output/`.
