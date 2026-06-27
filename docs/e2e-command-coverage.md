# E2E Command Coverage

This file is the command behavior coverage manifest for the E2E suite. `CommandBehaviorCoverageE2eTests` checks that every documented command has an owner row here, that every owner file exists, and that each row declares valid coverage dimensions.

Rows ending in `*` cover the command group and all documented child commands below that prefix. This is a coverage ownership map, not a claim that every option permutation is complete. Add more focused rows when a command grows behavior that needs its own E2E file.

Allowed dimensions: `artifact`, `browser`, `cleanup`, `failure`, `gif`, `help`, `network`, `report`, `state`, `success`, `trace`, `visual`.

Minimum rules:

- Every row must declare at least one of `success`, `failure`, or `help`.
- Network command rows must declare `network`.
- Capture command rows must declare both `artifact` and `visual`.
- `browser control script` must declare both `gif` and `trace`.
- `run` must declare `report`, `gif`, and `trace`.

| Command pattern | E2E coverage owner | Coverage dimensions | Coverage notes |
| --- | --- | --- | --- |
| `api` | `ApiCommandE2eTests.cs` | `help`, `success`, `failure` | Group help and API command surface. |
| `api request` | `ApiCommandE2eTests.cs` | `success`, `failure`, `artifact`, `network` | Query, headers, bodies, auth, output files, status checks, and failures. |
| `browser` | `BrowserLifecycleE2eTests.cs` | `help`, `browser`, `state` | Group help and browser lifecycle surface. |
| `browser launch` | `BrowserLifecycleE2eTests.cs` | `success`, `failure`, `browser`, `state`, `cleanup` | Headless launch, custom ports, validation, and state cleanup. |
| `browser close` | `BrowserLifecycleE2eTests.cs` | `success`, `failure`, `browser`, `state`, `cleanup` | Port-specific close, no-running behavior, and ignored args. |
| `browser app` | `BrowserLifecycleE2eTests.cs` | `help`, `success`, `failure`, `browser`, `state` | App group help plus app attach/launch surface. |
| `browser app attach` | `BrowserLifecycleE2eTests.cs` | `success`, `failure`, `browser`, `state` | Attach validation and real debug-endpoint attach. |
| `browser app launch` | `BrowserLifecycleE2eTests.cs` | `failure`, `browser`, `state` | Launch validation, missing executable, and debug endpoint failures. |
| `browser control` | `BrowserControlCommandE2eTests.cs`, `BrowserControlCommandSurfaceE2eTests.cs` | `help`, `success`, `failure`, `browser` | Control group help and broad control command smoke. |
| `browser control accessibility *` | `BrowserClockAccessibilityAliasE2eTests.cs` | `success`, `failure`, `browser`, `state` | Accessibility snapshot and accessible-role assertions. |
| `browser control assertions *` | `BrowserAssertionAliasCommandE2eTests.cs` | `success`, `failure`, `browser`, `state` | Provider-style assertion aliases and failure reasons. |
| `browser control capture *` | `BrowserCaptureCommandE2eTests.cs` | `success`, `failure`, `artifact`, `visual`, `browser` | Screenshots, PDFs, visual baselines, masks, clips, and validation. |
| `browser control clock *` | `BrowserClockAccessibilityAliasE2eTests.cs` | `success`, `failure`, `browser`, `state` | Clock install, tick, restore, and aliases. |
| `browser control context *` | `BrowserContextCommandE2eTests.cs`, `BrowserContextAliasCommandE2eTests.cs` | `success`, `failure`, `browser`, `state`, `cleanup` | Context lifecycle, permissions, emulation, service-worker policy, and aliases. |
| `browser control coverage *` | `BrowserCoverageCommandE2eTests.cs` | `success`, `failure`, `artifact`, `browser` | Coverage start/stop, stdout JSON, files, and validation. |
| `browser control events *` | `BrowserEventCommandE2eTests.cs`, `BrowserEventAliasCommandE2eTests.cs` | `success`, `failure`, `browser`, `state`, `artifact` | Console, page errors, dialogs, downloads, and event waits. |
| `browser control frames *` | `BrowserFrameCommandE2eTests.cs` | `success`, `failure`, `browser`, `state` | Frame actions, getters, waits, assertions, and missing-frame diagnostics. |
| `browser control input *` | `BrowserInputAliasCommandE2eTests.cs`, `BrowserAdvancedInputCommandE2eTests.cs` | `success`, `failure`, `browser`, `state`, `artifact` | Pointer, keyboard, form, drag/drop, uploads, clipboard, scroll, and downloads. |
| `browser control navigation *` | `BrowserNavigationRuntimeAliasE2eTests.cs` | `success`, `failure`, `browser`, `state` | Navigation aliases, waits, page state, and content commands. |
| `browser control network *` | `BrowserNetworkCommandE2eTests.cs`, `BrowserNetworkAliasCommandE2eTests.cs` | `success`, `failure`, `browser`, `network`, `artifact` | Routes, HAR, waits, mocked failures, headers, offline mode, WebSocket commands, and validation. |
| `browser control page *` | `BrowserNavigationRuntimeAliasE2eTests.cs`, `BrowserRuntimeInjectionE2eTests.cs` | `success`, `failure`, `browser`, `state` | Page utilities, runtime evaluation, injected scripts/styles, exposed functions, and element getters. |
| `browser control script` | `BrowserScriptE2eTests.cs` | `success`, `failure`, `browser`, `gif`, `trace`, `state` | Direct script execution, stdin, GIFs, traces, variables, and failure output. |
| `browser control storage *` | `BrowserStorageCommandE2eTests.cs` | `success`, `failure`, `browser`, `state`, `artifact` | Local/session storage, cookies, storage state, and validation. |
| `browser control tabs *` | `BrowserTabCommandE2eTests.cs` | `success`, `failure`, `browser`, `state`, `cleanup` | Tab list/open/wait/activate/close and popup waits. |
| `browser control validateScript` | `BrowserValidateScriptCommandE2eTests.cs` | `success`, `failure` | Script validation without a launched browser. |
| `browser control wait *` | `BrowserWaitCommandE2eTests.cs` | `success`, `failure`, `browser`, `state` | Selector, element, function, timeout, and auto waits. |
| `browser control workers *` | `BrowserWorkerCommandE2eTests.cs` | `success`, `failure`, `browser`, `state`, `network` | Worker list/wait/evaluate/intercept and failure diagnostics. |
| `files` | `FilesCommandE2eTests.cs` | `help`, `success`, `failure`, `artifact` | Group help and local file command surface. |
| `files *` | `FilesCommandE2eTests.cs` | `success`, `failure`, `artifact`, `state` | Read/write/append/expect/fixture aliases and failures. |
| `run` | `CmgRunE2eTests.cs`, `CmgRunAdvancedE2eTests.cs`, `CmgRunConfigProjectE2eTests.cs`, `CmgRunProviderDeclarationE2eTests.cs`, `CmgRunValidationE2eTests.cs` | `success`, `failure`, `artifact`, `report`, `gif`, `trace`, `browser`, `state` | Structured runner execution, reports, traces, GIFs, config/projects, sharding, tags, hooks, retries, declarations, and validation. |
