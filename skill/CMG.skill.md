# CMG Skill

CMG is a command-line browser control tool intended to be called by AI agents.

## How Agents Should Use CMG

- Start a controlled browser with `cmg browser launch` before page control commands.
- Close the controlled browser with `cmg browser close` when finished.
- Use `cmg browser control <action>` for one-off actions.
- Use `cmg browser control script --file <path>` for multi-step flows.
- Use `cmg browser control script --file -` to pipe a generated `.cmgscript` from stdin.
- Prefer scripts whenever doing more than one action on a page. A script gives the agent one parseable run, one exit code, deterministic ordering, and optional GIF recording.
- Add `--gif <path>` to script runs when a visual recording is useful.
- Treat stdout as parseable command output and stderr as failure diagnostics.
- Check exit code `0` for success and `1` for failures.
- Prefer selectors that work in the browser, such as `#id`, `.class`, `[data-name='value']`, and combined CSS selectors.
- For screenshots without `--output` or `output=`, expect `data:image/png;base64,...`.
- For `getElement --screenshot`, the screenshot result is also a `data:image/png;base64,...` URL unless `--output` is used.
- Use `waitForElement` before interacting with dynamic UI.
- Use `.cmgscript` block `dragAndDrop` when a drag needs intermediate `delay`, `hover`, or `waitForElement` steps.
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
