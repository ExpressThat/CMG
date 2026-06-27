# `browser control context`

Browser context, emulation, permission, and environment commands.

```powershell
cmg browser control context [command] [options]
```

## Subcommands

- [`emulate`](emulate.md): Apply page environment and viewport emulation.
- [`emulateMedia`](emulateMedia.md): Apply page media emulation.
- [`setGeolocation`](setGeolocation.md): Set page-visible geolocation.
- [`grantPermissions`](grantPermissions.md): Grant page-side permissions.
- [`clearPermissions`](clearPermissions.md): Clear page-side permission grants.
- [`setJavaScriptEnabled`](setJavaScriptEnabled.md): Enable or disable dynamic JavaScript execution.
- [`javaScriptEnabled`](javaScriptEnabled.md): Enable or disable dynamic JavaScript execution.
- [`bypassCSP`](bypassCSP.md): Enable or disable page-side CSP bypass.
- [`serviceWorkers`](serviceWorkers.md): Allow or block service worker registration.
- [`setServiceWorkers`](setServiceWorkers.md): Allow or block service worker registration.
- [`clear`](clear.md): Clear storage, cookies, caches, IndexedDB, and service workers.
- [`clearContext`](clearContext.md): Clear storage, cookies, caches, IndexedDB, and service workers.
- [`reset`](reset.md): Clear context state and navigate to `about:blank`.
- [`resetContext`](resetContext.md): Clear context state and navigate to `about:blank`.
- [`browserContexts`](browserContexts/index.md): Isolated browser context commands.

## Behavior

- Requires a browser started with [`browser launch`](../../launch.md).
- Runs the same underlying scripting actions as `browser control script`.
- Writes `PASS` and context or environment output lines to stdout.
- Writes browser, argument, parse, or action errors to stderr.
- Exits `0` on success and `1` on failure.

## Examples

```powershell
cmg browser control context emulate --width 390 --height 844 --mobile --touch --locale en-GB
cmg browser control context emulate --device "Pixel 7" --timezone Europe/London
cmg browser control context emulateMedia --media print --color-scheme dark
cmg browser control context setGeolocation 51.5 -0.1 --accuracy 10
cmg browser control context grantPermissions geolocation notifications
cmg browser control context javaScriptEnabled true
cmg browser control context setServiceWorkers block
cmg browser control context browserContexts new --url "about:blank"
```
