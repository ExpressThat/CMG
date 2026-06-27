# E2E Testing

CMG has a real browser-backed E2E test project at `tests/CMG.E2E.Tests`.

The E2E project runs the CLI as an external process with:

```powershell
dotnet test tests\CMG.E2E.Tests\CMG.E2E.Tests.csproj /p:UseSharedCompilation=false
```

For repeat local runs after a successful build, use:

```powershell
dotnet test tests\CMG.E2E.Tests\CMG.E2E.Tests.csproj --no-build
```

## What It Does

- Launches the built CMG app as an external process.
- Builds CMG once through the E2E test project's `ProjectReference`; individual E2E CLI calls reuse the built apphost (`CMG.exe` when available) rather than invoking `dotnet run`.
- Starts Chrome headless with `cmg browser --port <free-port> launch --headless`.
- Uses an isolated temporary `LOCALAPPDATA` directory for the shared browser E2E fixture so tests do not touch a developer's normal CMG browser state.
- Starts a tiny local static HTTP fixture server for the shared browser fixture for origin-sensitive behavior such as cookies, network, workers, and future request tests.
- Reuses the same C# fixture server for API command tests through `/api/echo`, `/api/status/<code>`, and `/api/slow`.
- Drives the browser through real CLI commands, direct `.cmgscript` files, and `cmg run` test files.
- Writes and verifies real screenshots, GIFs, traces, JSON reports, HTML reports, and JUnit reports.
- Uses `tests/CMG.E2E.Tests/Fixtures/index.html` as the main page fixture.

## Fixture Page

The fixture page is intentionally broad. It includes:

- Form controls, checkboxes, radios, selects, textareas, and file inputs.
- Buttons, text targets, hidden/visible states, editable content, classes, styles, and accessibility labels.
- Hover targets, drag/drop targets, scroll containers, canvas, iframes, dialogs, local storage, fetch, and workers.

Add page affordances here when a new command or script action needs real browser state.

## Coverage Direction

The project is intended to grow until every command, every script action, and every meaningful failure path has E2E coverage. Unit tests still cover parser/lowering edge cases, but command/action behavior should graduate into E2E coverage when it depends on a browser, artifacts, process state, reports, GIFs, or real CLI output.

Keep E2E tests explicit and scenario-shaped:

- One C# test file per command or feature area.
- C# files must stay under 250 lines.
- Large `.cmgscript` scenarios may exceed 250 lines when needed, but keep one scenario per file.
- If a script is correct and an E2E test exposes an unexpected failure, fix the CMG root cause rather than forcing the script around the bug.
- Prefer real headless browser checks over mocks.
- Verify parseable stdout, stderr, exit codes, and generated artifacts.

## Speed Notes

- `dotnet test` builds CMG once through the E2E project reference. The fixture then reuses the built apphost for individual CLI calls.
- `--no-build` is the fastest local loop when code has already been built.
- Browser E2E tests use the shared `CmgE2eCollection` fixture. The fixture owns one Chrome process, remote debugging port, browser profile, `LOCALAPPDATA` root, output directory, and fixture server for the browser-backed collection.
- The E2E assembly disables xUnit test parallelization. The shared browser fixture is isolated from local developer state, and full-suite runs are serial to avoid shared DevTools/process-cleanup contention while commands are spawning external CMG processes.
- Fixture CLI calls automatically scope `browser` commands with `browser --port <fixture-port>` and `run` commands with `--browser-port <fixture-port>`.
- The fixture closes the selected browser port at disposal and falls back to killing any process ids left in CMG state files before deleting its workspace.
- Non-browser E2E classes, such as help coverage and local file commands, use a lightweight CLI fixture and do not launch Chrome.
- Keep independent browser scenarios in separate E2E classes when they do not need to share browser state. Smaller feature-shaped classes make focused local runs faster and easier to reason about.

## Current Seed Coverage

The first E2E slice covers:

- Browser lifecycle launch/close behavior.
- App attach validation failure, successful attach to a real debugging endpoint, and app launch missing-executable failure output.
- All documented leaf commands expose working `--help` output.
- A broad browser action-surface script covering navigation, input, assertions, dialogs, frames, storage, emulation, clocks, looping, branching, try/catch, and trace output.
- Artifact and state actions for visual assertions, screenshots, PDFs, coverage, storage state, local file actions, and tab control.
- Browser-control CLI behavior for waits, runtime getters, input, assertions, storage, cookies, context/emulation, isolated browser contexts, clock, accessibility, frame actions/getters/failure reasons, tabs, and capture artifacts.
- Browser-control clock/accessibility aliases for `restoreClock`, `accessibilitySnapshot`, `expectAccessible`, and accessibility failure reasons.
- Browser-control context aliases for JavaScript toggling, service-worker mode, context clear, and context reset.
- Script environment actions for locale, timezone, media, geolocation, permission grants, permission clearing, and validation failure output.
- Browser-control storage lifecycle commands for local/session storage remove/clear, cookie attributes/remove/clear, storage-state save/load, and validation failures.
- Browser-control wait command aliases for selector states, function waits, fixed waits, auto waits, and selector-state failure reasons.
- Browser-control navigation/runtime aliases for `expectUrl`, `waitForNavigation`, `waitForNetworkIdle`, `evalOnSelector`, and `evalAll`.
- Browser-control provider-style assertion aliases for text absence, state assertions, values, attributes, class/id/CSS/property, accessibility name, role, checked state, and counts.
- Browser-control assertion alias coverage for direct CLI text, body text, state, eval, value, attribute, accessibility, checked, and count variants.
- Runtime setup actions for init scripts, exposed functions/bindings, script/style tag injection, generated page content, HTML reads, and bounding boxes.
- Browser-control network commands for routes, HAR export/replay, network waits, mocked failures, headers, and offline mode.
- Browser-control network environment aliases for extra headers, HTTP credentials, proxy rewrites, mocked responses, and validation failures.
- Browser-control WebSocket commands for route installation, socket waits, message waits, generic event waits, and route clearing against a real WebSocket handshake.
- Browser-control event commands for console messages, page errors, generic event waits, dialog capture, dialog behavior, and absence assertions.
- Browser-control event aliases for console capture/waits, page-error capture/waits, generic `waitForEvent`, and dialog `onDialog`/`handleDialog`.
- Browser-control coverage commands for start/stop aliases, file-backed coverage output, stdout JSON output, and validation failures.
- Browser-control tab alias commands for listing, opening, waiting, popup waits, activation, closing, and tab-count failure reasons.
- Browser-control capture commands for element screenshots, direct element screenshot output, page clips, temporary styles, masks, visual assertion aliases, PDF options, and validation failures.
- Browser-control advanced input commands for clipboard shims, tap/touch tap, mouse movement/buttons, scroll/wheel, custom events, file upload aliases, and download wait success/failure behavior.
- Browser-control input aliases for double/right click, sequential typing, selection, blur, key down/up, hotkeys, select options, drag aliases, low-level mouse aliases, scroll aliases, and select-file upload.
- Browser-control worker commands for real worker listing, waiting, evaluation, interception, and missing-worker wait failure output.
- Local `files` command success and failure behavior.
- API request command behavior for query parameters, headers, JSON/form/raw bodies, basic auth, status matching, output files, response headers, validation failures, and timeout failures.
- Script and runner `apiRequest` action behavior for query parameters, headers, status matching, output files, JSON bodies, and step failure diagnostics.
- Navigation aliases and page state commands, including `goto`, `visit`, history back/forward, reload, URL/title waits and assertions, page content reads, `setContent`, runtime text reads, input, assertions, screenshots, element HTML, and failure reasons.
- Page utility aliases for viewport sizing, captions/message bars, temporary highlights, and explicit delays.
- Direct script execution with GIF and trace output.
- Direct script and runner import expansion for validation, imported macros, returned values, and missing-import failure output.
- GIF block actions for direct `gif`, provider-style `recordVideo`/`screencast` aliases, and command-level GIF suppression in direct scripts and `cmg run`.
- Script trace actions for partial trace files, command-level trace suppression, and failure-time partial trace output.
- Dialog handling, variables, macros, logic, `set` capture, and block `return`.
- Scoped locator behavior for rich locator filters, shadow DOM locators, `within`, and scoped `foreachSelector` in direct scripts and `cmg run`.
- Advanced script control flow for `for`, `foreach`, `foreachJson`, `foreachSelector`, `while`, `until`, `doWhile`, `doUntil`, `break`, `continue`, `retry`, `toPass`, and scoped timeout blocks.
- Structured `cmg run` with reports, traces, optional GIFs, tags, variables, list mode, hooks, parameterized tests, runtime skips, soft assertions, retries, and max-failure stopping.
- Runner config/project execution for relative artifact paths, project names, project variables, CLI variable overrides, repeat scheduling, sharding, and missing-project failure behavior.
- Runner provider declarations for `.only`, `.skip`, `.fixme`, `.todo`, suite skip inheritance, and report annotations across JSON, HTML, and JUnit artifacts.

Worker evaluation and interception are covered against real headless Chrome workers created from the fixture server after CMG initializes worker support.
