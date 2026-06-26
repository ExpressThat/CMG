# `browser control network`

Network routing, waits, HAR, WebSocket, and page-side network environment command group.

```powershell
cmg browser control network [command] [options]
```

## Subcommands

- [`route`](route.md): Install a fetch/XHR route.
- [`intercept`](intercept.md): Alias a fetch/XHR route as an intercept.
- [`mockResponse`](mockResponse.md): Alias a fetch/XHR route as a mocked response.
- [`clearRoutes`](clearRoutes.md): Clear network routes.
- [`waitForRequest`](waitForRequest.md): Wait for a matching request.
- [`waitForRequestFinished`](waitForRequestFinished.md): Wait for a matching completed request.
- [`waitForRequestFailed`](waitForRequestFailed.md): Wait for a matching failed request.
- [`waitForResponse`](waitForResponse.md): Wait for a matching response.
- [`exportHar`](exportHar.md): Export recorded page network traffic.
- [`replayHar`](replayHar.md): Replay responses from a HAR file.
- [`setHeaders`](setHeaders.md): Set extra HTTP headers.
- [`setExtraHTTPHeaders`](setExtraHTTPHeaders.md): Set extra HTTP headers.
- [`clearHeaders`](clearHeaders.md): Clear extra HTTP headers.
- [`clearExtraHTTPHeaders`](clearExtraHTTPHeaders.md): Clear extra HTTP headers.
- [`setCredentials`](setCredentials.md): Set page-side HTTP credentials.
- [`setHttpCredentials`](setHttpCredentials.md): Set page-side HTTP credentials.
- [`httpCredentials`](httpCredentials.md): Set page-side HTTP credentials.
- [`authenticate`](authenticate.md): Set page-side HTTP credentials.
- [`clearCredentials`](clearCredentials.md): Clear HTTP credentials.
- [`clearHttpCredentials`](clearHttpCredentials.md): Clear HTTP credentials.
- [`setProxy`](setProxy.md): Set a page-side fetch/XHR proxy rewrite.
- [`proxy`](proxy.md): Set a page-side fetch/XHR proxy rewrite.
- [`clearProxy`](clearProxy.md): Clear the page-side proxy rewrite.
- [`setOffline`](setOffline.md): Enable or disable page-side offline simulation.
- [`webSocket`](webSocket/index.md): WebSocket route and wait commands.

## Behavior

- Requires a browser started with [`browser launch`](../../launch.md).
- Runs the same underlying scripting actions as `browser control script`.
- Network routing and environment controls are page-side patches for fetch/XHR/WebSocket behavior.
- Writes `PASS` and parseable network output lines to stdout.
- Writes browser, argument, timeout, file, parse, or action errors to stderr.
- Exits `0` on success and `1` on failure.

## Examples

```powershell
cmg browser control network route "/api/profile" --status 200 --body "{\"name\":\"CMG\"}"
cmg browser control network intercept "/api/profile" --method GET --status 200
cmg browser control network mockResponse "/api/profile" --status 200 --body "{\"name\":\"CMG\"}"
cmg browser control network waitForResponse "/api/profile" --status 200 --timeout 5000
cmg browser control network waitForResponse "/api/profile/\d+" --match regex --ignore-case
cmg browser control network setHeaders X-CMG-Agent true Accept application/json
cmg browser control network webSocket wait "/socket"
```
