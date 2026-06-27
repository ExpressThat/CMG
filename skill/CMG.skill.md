# CMG Skill

CMG is a command-line browser control tool intended to be called by AI agents.

## Agent Quick Contract

Use CMG when you need a real browser action, a repeatable browser script, a structured test run, or visual evidence such as screenshots, GIFs, traces, and reports.

1. Start or select a browser.
   - Chrome is the default: `cmg browser launch`.
   - Use `cmg --edge browser launch` or `cmg --firefox browser launch` for other browsers.
   - Use the same top-level browser option for launch, control, run, and close.
2. Choose the smallest surface that fits the task.
   - One action: `cmg browser control <group> <command> ...`.
   - Multi-step direct automation: `cmg browser control script --file flow.cmgscript`.
   - Generated script from stdin: `cmg browser control script --file -`.
   - Structured tests, retries, reports, traces, sharding, or per-test GIFs: `cmg run <path>`.
   - Add `--auto-launch` to `cmg run` when the runner should start the selected browser if it is not already running. Add `--headless` with `--auto-launch` for CI or background runs.
3. Capture useful artifacts when the user needs evidence.
   - Direct script GIF: `cmg browser control script --file flow.cmgscript --gif artifacts/flow.gif`.
   - Runner GIFs: `cmg run tests --gif artifacts/gifs`.
   - Runner reports/traces: `cmg run tests --report-json artifacts/report.json --trace artifacts/traces`.
   - Browser diagnostics are armed automatically by `cmg browser launch`, `cmg browser app launch`, and `cmg browser app attach`.
   - After risky UI actions, inspect captured browser fallout with `listPageErrors` and `listConsole level=error`, then gate with `expectNoPageError timeout=250` and `expectNoConsole level=error timeout=250`.
   - `captureConsole` and `capturePageErrors` are deprecated compatibility aliases for ensuring capture is installed; they are usually unnecessary and do not clear existing entries.
4. Parse results predictably.
   - Exit code `0` means success; exit code `1` means failure.
   - Treat stdout as parseable command output: `PASS 001 line=3 action=navigate ...`; nested macro/loop/step output adds `context="..."`.
   - For runner reports, prefer JSON step fields (`sequence`, `lineNumber`, `context`, `action`) instead of parsing human output strings. JSON/HTML steps are public executed steps only; trace artifacts keep raw internals.
   - Treat stderr as diagnostics, including failed step/test reasons.
5. Use safe paths.
   - Write artifacts under an explicit workspace folder such as `artifacts/` or `demo-output/`.
   - Prefer relative paths in scripts and examples unless the user gave an absolute path.
   - Use `--output` or `output=` for file artifacts; otherwise screenshot commands may print `data:image/png;base64,...`.
6. Close the browser when finished: `cmg browser close`.

For extra detail in this generated skill file:

- See `## Source: README.md` for the product overview and common workflows.
- See `## Source: docs/quick-start.md` for the fastest launch, run, and artifact examples.
- See `## Source: docs/commands.md` and `## Source: docs/commands/run.md` for exact CLI arguments, stdout/stderr, exit codes, and examples.
- See `## Source: docs/scripting/index.md`, `## Source: docs/scripting/syntax.md`, and `## Source: docs/scripting/actions.md` for `.cmgscript` syntax and action behavior.
- See `## Source: docs/scripting/gif-recording.md` for GIF and visual evidence rules.
- See `## Source: demo-scripts/README.md` and the later demo script sources for runnable examples.

## How Agents Should Use CMG

- Start a controlled browser with `cmg browser launch` before page control commands.
- Close the controlled browser with `cmg browser close` when finished.
- Chrome is the default browser. `--chrome` is available but optional, `--edge` targets Microsoft Edge, and `--firefox` targets Firefox. Put the browser option before the command group, such as `cmg --edge browser launch` or `cmg --firefox browser control script --file flow.cmgscript`.
- Use the same browser option for launch, control, and close commands within a flow. For example, an Edge flow should use `cmg --edge browser launch`, `cmg --edge browser control script --file flow.cmgscript`, and `cmg --edge browser close`.
- Use `cmg browser control <action>` for one-off actions.
- Use `cmg browser control script --file <path>` for multi-step flows.
- Use `cmg browser control script --file -` to pipe a generated `.cmgscript` from stdin.
- Use `cmg browser control script --inline "<script>"` for short generated scripts when quoting is practical.
- Prefer scripts whenever doing more than one action on a page. A script gives the agent one parseable run, one exit code, deterministic ordering, and optional GIF recording.
- Add `--gif <path>` to script runs when a visual recording is useful.
- Treat stdout as parseable command output and stderr as failure diagnostics.
- For browser-side failures after visual testing, prefer `listPageErrors` and `listConsole level=error` to inspect captured events. CMG captures only from launch/attach/arming time forward; it cannot recover earlier browser history.
- Check exit code `0` for success and `1` for failures.
- Prefer selectors that work in the browser, such as `#id`, `.class`, `[data-name='value']`, and combined CSS selectors.
- For screenshots without `--output` or `output=`, expect `data:image/png;base64,...`.
- For `getElement --screenshot`, the screenshot result is also a `data:image/png;base64,...` URL unless `--output` is used.
- Use `waitForElement` before interacting with dynamic UI.
- CMG automatically accepts browser JavaScript dialogs and leave-page prompts while connected to a page, including alerts, confirms, prompts, and unsaved-changes before-unload prompts.
- User-like actions such as `click`, `type`, `clear`, `hover`, `select`, and `dragAndDrop` do not scroll automatically. Add explicit `scrollIntoView` steps before interacting with elements outside the current viewport. Screenshots are the exception and may scroll the selected element into view for capture.
- Use `.cmgscript` block `dragAndDrop` when a drag needs intermediate `delay`, `hover`, or `waitForElement` steps.
- In `--gif` scripts, all automatic virtual pointer movement dispatches browser mouse and pointer movement events, including movement before `click`, `type`, `clear`, `hover`, and `select`.
- Use script-only `moveMouse "bottom"` plus `delay` inside a GIF `dragAndDrop` block when a page auto-scrolls while a dragged item is held near the viewport edge. For scrollable app containers, prefer `moveMouse selector=".content-area" edge=bottom inset=24`. CMG keeps page drag state active with pointer/mouse down, held move, and up events during block drags. `moveMouse` has no one-off CLI command and requires `--gif`.
- CMG does not force `DataTransfer.effectAllowed`, `dropEffect`, or payloads during synthetic drags. The page's own `dragstart` handler should set those values; CMG preserves the page-set values through later drag events.
- In GIF drags, CMG uses one synthetic drag lifecycle and dispatches one `drop` event. CMG should not run a second native or fallback drop after the recorded drop completes.
- Use `showMessageBar "message"` to place a visible centered caption bar near the top of the page while recording. It dynamically sizes to the message, supports multi-line captions, and appears above page dialogs.
- During GIF drag recording, page-owned custom drag images take precedence. If the page does not call `DataTransfer.setDragImage()`, CMG shows a browser-default style preview bridge so the drag remains visible in the live browser and recorded GIF.

## Authentication Workflows

If a page requires user authentication, do not try to automate credentials unless the user explicitly asks for that and provides a safe method. Instead:

1. Launch the CMG-controlled browser.
2. Navigate it to the page where the user needs to sign in.
3. Tell the user to complete the login manually in that browser window.
4. Wait for the user to confirm sign-in is complete.
5. Continue with `waitForElement` and the rest of the page automation.

## Release Contents

This skill file is generated during the release workflow from this source preamble, repository documentation, and demo scripts. Do not edit generated release copies by hand; update this file or the source docs instead.
