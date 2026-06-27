# E2E Command Coverage

This file is the command behavior coverage manifest for the E2E suite. `CommandBehaviorCoverageE2eTests` checks that every documented command has an owner row here and that every owner file exists.

Rows ending in `*` cover the command group and all documented child commands below that prefix. This is a coverage ownership map, not a claim that every option permutation is complete. Add more focused rows when a command grows behavior that needs its own E2E file.

| Command pattern | E2E coverage owner | Coverage notes |
| --- | --- | --- |
| `api` | `ApiCommandE2eTests.cs` | Group help and API command surface. |
| `api request` | `ApiCommandE2eTests.cs` | Query, headers, bodies, auth, output files, status checks, and failures. |
| `browser` | `BrowserLifecycleE2eTests.cs` | Group help and browser lifecycle surface. |
| `browser launch` | `BrowserLifecycleE2eTests.cs` | Headless launch, custom ports, validation, and state cleanup. |
| `browser close` | `BrowserLifecycleE2eTests.cs` | Port-specific close, no-running behavior, and ignored args. |
| `browser app` | `BrowserLifecycleE2eTests.cs` | App group help plus app attach/launch surface. |
| `browser app attach` | `BrowserLifecycleE2eTests.cs` | Attach validation and real debug-endpoint attach. |
| `browser app launch` | `BrowserLifecycleE2eTests.cs` | Launch validation, missing executable, and debug endpoint failures. |
| `browser control` | `BrowserControlCommandE2eTests.cs`, `BrowserControlCommandSurfaceE2eTests.cs` | Control group help and broad control command smoke. |
| `browser control accessibility *` | `BrowserClockAccessibilityAliasE2eTests.cs` | Accessibility snapshot and accessible-role assertions. |
| `browser control assertions *` | `BrowserAssertionAliasCommandE2eTests.cs` | Provider-style assertion aliases and failure reasons. |
| `browser control capture *` | `BrowserCaptureCommandE2eTests.cs` | Screenshots, PDFs, visual baselines, masks, clips, and validation. |
| `browser control clock *` | `BrowserClockAccessibilityAliasE2eTests.cs` | Clock install, tick, restore, and aliases. |
| `browser control context *` | `BrowserContextCommandE2eTests.cs`, `BrowserContextAliasCommandE2eTests.cs` | Context lifecycle, permissions, emulation, service-worker policy, and aliases. |
| `browser control coverage *` | `BrowserCoverageCommandE2eTests.cs` | Coverage start/stop, stdout JSON, files, and validation. |
| `browser control events *` | `BrowserEventCommandE2eTests.cs`, `BrowserEventAliasCommandE2eTests.cs` | Console, page errors, dialogs, downloads, and event waits. |
| `browser control frames *` | `BrowserFrameCommandE2eTests.cs` | Frame actions, getters, waits, assertions, and missing-frame diagnostics. |
| `browser control input *` | `BrowserInputAliasCommandE2eTests.cs`, `BrowserAdvancedInputCommandE2eTests.cs` | Pointer, keyboard, form, drag/drop, uploads, clipboard, scroll, and downloads. |
| `browser control navigation *` | `BrowserNavigationRuntimeAliasE2eTests.cs` | Navigation aliases, waits, page state, and content commands. |
| `browser control network *` | `BrowserNetworkCommandE2eTests.cs`, `BrowserNetworkAliasCommandE2eTests.cs` | Routes, HAR, waits, mocked failures, headers, offline mode, WebSocket commands, and validation. |
| `browser control page *` | `BrowserNavigationRuntimeAliasE2eTests.cs`, `BrowserRuntimeInjectionE2eTests.cs` | Page utilities, runtime evaluation, injected scripts/styles, exposed functions, and element getters. |
| `browser control script` | `BrowserScriptE2eTests.cs` | Direct script execution, stdin, GIFs, traces, variables, and failure output. |
| `browser control storage *` | `BrowserStorageCommandE2eTests.cs` | Local/session storage, cookies, storage state, and validation. |
| `browser control tabs *` | `BrowserTabCommandE2eTests.cs` | Tab list/open/wait/activate/close and popup waits. |
| `browser control validateScript` | `BrowserValidateScriptCommandE2eTests.cs` | Script validation without a launched browser. |
| `browser control wait *` | `BrowserWaitCommandE2eTests.cs` | Selector, element, function, timeout, and auto waits. |
| `browser control workers *` | `BrowserWorkerCommandE2eTests.cs` | Worker list/wait/evaluate/intercept and failure diagnostics. |
| `files` | `FilesCommandE2eTests.cs` | Group help and local file command surface. |
| `files *` | `FilesCommandE2eTests.cs` | Read/write/append/expect/fixture aliases and failures. |
| `run` | `CmgRunE2eTests.cs`, `CmgRunAdvancedE2eTests.cs`, `CmgRunConfigProjectE2eTests.cs`, `CmgRunProviderDeclarationE2eTests.cs`, `CmgRunValidationE2eTests.cs` | Structured runner execution, reports, traces, GIFs, config/projects, sharding, tags, hooks, retries, declarations, and validation. |
