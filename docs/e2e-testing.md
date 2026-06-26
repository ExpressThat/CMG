# E2E Testing

CMG has a real browser-backed E2E test project at `tests/CMG.E2E.Tests`.

The E2E project runs the CLI as an external process with:

```powershell
dotnet test tests\CMG.E2E.Tests\CMG.E2E.Tests.csproj /p:UseSharedCompilation=false
```

## What It Does

- Launches the built CMG app as an external process.
- Starts Chrome headless with `cmg browser launch --headless`.
- Uses an isolated temporary `LOCALAPPDATA` directory so tests do not touch a developer's normal CMG browser state.
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

## Current Seed Coverage

The first E2E slice covers:

- Browser lifecycle launch/close behavior.
- App attach validation failure.
- All documented leaf commands expose working `--help` output.
- A broad browser action-surface script covering navigation, input, assertions, dialogs, frames, storage, emulation, clocks, looping, branching, try/catch, and trace output.
- Artifact and state actions for visual assertions, screenshots, PDFs, coverage, storage state, local file actions, and tab control.
- Local `files` command success and failure behavior.
- Navigation, runtime text reads, input, assertions, screenshots, element HTML, and failure reasons.
- Direct script execution with GIF and trace output.
- Dialog handling, variables, macros, logic, `set` capture, and block `return`.
- Structured `cmg run` with reports, traces, optional GIFs, tags, variables, and list mode.
