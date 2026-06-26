# CMG

CMG is a browser automation CLI for producing choreographed, human-readable, pointer-accurate visual evidence.

It drives a real browser from scripts or one-shot CLI commands, records animated GIFs with a live virtual pointer, and reports exactly which step failed and why. It is built for AI agents and people who need automation runs that are easy to inspect, explain, replay, and share.

## Why CMG

Most browser automation tools prove that something happened. CMG is designed to show how it happened.

- **Pointer-accurate GIFs**: clicks, hovers, typing, scrolls, drags, offsets, captions, and drag ghosts are recorded through CMG's virtual pointer path.
- **Visual evidence by default when you ask for it**: use `--gif` for a whole run, or a `gif { ... }` block for a focused moment.
- **Readable scripts**: `.cmgscript` is intentionally compact, whitespace-tolerant, and friendly to AI-generated browser control.
- **Tests and browser control**: use `cmg run` for structured tests, reports, retries, traces, and sharding; use `browser control script` when an agent needs to control the browser directly.
- **Useful failures**: failed steps include the action, line number, and reason in stderr, reports, and traces.
- **Real automation surface**: navigation, locators, assertions, dialogs, frames, network waits/mocks, storage, contexts, workers, files, screenshots, PDF output, accessibility, and visual assertions are documented as script actions and CLI command groups.

CMG is not trying to be a clone of Cypress, Puppeteer, or Playwright. It gives you the automation coverage you expect, but its center of gravity is different: CMG creates inspectable visual evidence that reads like a guided browser performance.

## Install And Run Locally

From this repository:

```powershell
dotnet build /p:UseSharedCompilation=false
dotnet run -- browser launch
```

Published command examples use `cmg`:

```powershell
cmg browser launch
```

While developing in this repo, replace `cmg` with `dotnet run --`.

## Make Your First GIF

Create `first-gif.cmgscript`:

```text
navigate "C:\Projects\CMG\index.html"
waitForElement "#openProfileDialog" timeout=5000
step "Open the profile dialog" {
  click "#openProfileDialog"
}
type "#profileName" "My first CMG GIF"
```

Run it with GIF recording:

```powershell
cmg browser launch
cmg browser control script --file first-gif.cmgscript --gif demo-output\first-gif.gif
```

The GIF shows the visible browser viewport, the virtual pointer moving to the target, pointer events firing against the page, typed text appearing progressively, and the step caption.

## Write Your First Test

Create `first-test.cmgscript`:

```text
suite "profile dialog" {
  beforeEach {
    navigate "C:\Projects\CMG\index.html"
    waitForElement "#openProfileDialog" timeout=5000
  }

  test "opens and captures the dialog" tag=smoke {
    step "Open dialog" {
      click "#openProfileDialog"
    }

    expectVisible "#profileDialog"
    expectText "#lastDialogAction" "None"

    gif "dialog evidence" {
      type "#profileName" "CMG Test Profile"
      screenshot "#profileDialog" output="demo-output\profile-dialog.png"
    }
  }
}
```

Run the test with reports and whole-test GIFs:

```powershell
cmg run first-test.cmgscript --gif demo-output\gifs --report-html demo-output\report.html --report-json demo-output\report.json
```

When `--gif` is used, CMG records the entire selected test and suppresses nested `gif` block files. The nested actions still run and appear in the whole-test GIF.

## Script Or Test?

Use `browser control script` when you want direct browser automation:

```powershell
cmg browser control script --file flow.cmgscript --gif demo-output\flow.gif
```

Use `cmg run` when you want a test runner:

```powershell
cmg run tests\flows --grep checkout --retries 2 --trace demo-output\traces
```

Both script types use the same action syntax, control flow, macros, locators, GIF behavior, and virtual pointer model. The runner adds suites, hooks, selection, retries, sharding, reports, and per-test traces.

## Learn More

- [Docs overview](docs/README.md)
- [Command reference](docs/commands.md)
- [Scripting guide](docs/scripting/index.md)
- [Action index](docs/scripting/action-index.md)
- [GIF recording](docs/scripting/gif-recording.md)
- [Script style guide](docs/scripting/style-guide.md)
- [Runnable demos](demo-scripts/README.md)

## Documentation For Agents

CMG is intended to be called by AI agents. Command docs include arguments, options, stdout, stderr, exit codes, and examples. Scripting docs include syntax, action behavior, error handling, migration notes, and style guidance.

When adding or changing a command or scripting feature, update `docs/` in the same change.
