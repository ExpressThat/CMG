using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private IReadOnlyList<string> ExecuteAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        ScriptGifRecorder? recorder)
    {
        if (action.Children.Count > 0 &&
            !string.Equals(action.Name, "dragAndDrop", StringComparison.OrdinalIgnoreCase) &&
            !IsRecordingBlock(action.Name) &&
            !string.Equals(action.Name, "set", StringComparison.OrdinalIgnoreCase) &&
            !IsControlAction(action.Name))
        {
            throw new ScriptExecutionException($"Action '{action.Name}' does not accept a block body.");
        }

        return action.Name.ToLowerInvariant() switch
        {
            "navigate" or "goto" or "visit" => ExecuteNavigate(remoteDebuggingUrl, automationClient, action),
            "reload" or "goback" or "goforward" or "waitforurl" or "waitfortitle" or "expecturl" or "expecttitle" or "tohaveurl" or "tohavetitle" or
            "waitforloadstate" or "waitfornavigation" => ExecuteNavigationAction(remoteDebuggingUrl, automationClient, NormalizeNavigationAlias(action)),
            "waitforselector" or "waitforfunction" or "waitfortimeout" => ExecuteWaitAction(remoteDebuggingUrl, automationClient, action),
            "waitforelement" => ExecuteWaitForElement(remoteDebuggingUrl, automationClient, action),
            "wait" => ExecuteWaitAlias(remoteDebuggingUrl, automationClient, action),
            "assertvisible" => ExecuteWaitForElement(remoteDebuggingUrl, automationClient, action),
            "click" => ExecuteSelectorAction(remoteDebuggingUrl, automationClient, action, selector => automationClient.Click(remoteDebuggingUrl, selector)),
            "dblclick" or "doubleclick" or "rightclick" or "contextclick" => ExecuteMouseClickVariant(remoteDebuggingUrl, automationClient, action),
            "tap" or "touchtap" => ExecuteTap(remoteDebuggingUrl, automationClient, action, recorder),
            "presssequentially" => ExecuteType(remoteDebuggingUrl, automationClient, action with { Name = "type" }, recorder),
            "fill" => ExecuteFill(remoteDebuggingUrl, automationClient, action, recorder),
            "check" or "uncheck" or "focus" or "blur" or "selecttext" => ExecuteElementDomAction(remoteDebuggingUrl, automationClient, action),
            "dispatchevent" => ExecuteDispatchEvent(remoteDebuggingUrl, automationClient, action),
            "type" => ExecuteType(remoteDebuggingUrl, automationClient, action, recorder),
            "clear" => ExecuteSelectorAction(remoteDebuggingUrl, automationClient, action, selector => automationClient.Clear(remoteDebuggingUrl, selector)),
            "press" => ExecutePress(remoteDebuggingUrl, automationClient, action),
            "keydown" or "keyup" or "inserttext" => ExecuteKeyboardAction(remoteDebuggingUrl, automationClient, action),
            "setclipboard" or "writeclipboard" or "readclipboard" or "clearclipboard" => ExecuteClipboardAction(remoteDebuggingUrl, automationClient, action),
            "hover" => ExecuteSelectorAction(remoteDebuggingUrl, automationClient, action, selector => automationClient.Hover(remoteDebuggingUrl, selector)),
            "scrollintoview" => ExecuteSelectorAction(remoteDebuggingUrl, automationClient, action, selector => automationClient.ScrollElementIntoView(remoteDebuggingUrl, selector)),
            "select" or "selectoption" => ExecuteSelect(remoteDebuggingUrl, automationClient, action),
            "showmessagebar" or "caption" => ExecuteShowMessageBar(remoteDebuggingUrl, automationClient, action),
            "fail" => ExecuteFail(action),
            "delay" => ExecuteDelay(action),
            "html" => ExecuteHtml(remoteDebuggingUrl, automationClient, action),
            "textcontent" or "innertext" or "inputvalue" or "getattribute" => ExecuteElementGetter(remoteDebuggingUrl, automationClient, action),
            "screenshot" => ExecuteScreenshot(remoteDebuggingUrl, automationClient, action),
            "screenshotpage" => ExecuteScreenshotPage(remoteDebuggingUrl, automationClient, action),
            "printpdf" or "pdf" => ExecutePrintPdf(remoteDebuggingUrl, automationClient, action),
            "asserttext" or "expecttext" or "tohavetext" or "tocontaintext" or "containstext" or "contains" or "waitfortext" or
            "expectnotext" or "expectnottext" or "notcontains" or "notcontainstext" or "tonotcontaintext" or "tohavenotext" or "tohavenottext" =>
                ExecuteAssertText(remoteDebuggingUrl, automationClient, action),
            "expectvisible" or "tobevisible" or "waitforvisible" or "expecthidden" or "tobehidden" or "waitforhidden" or
            "expectenabled" or "tobeenabled" or "expectdisabled" or "tobedisabled" or
            "expectattached" or "tobeattached" or "expectdetached" or "tobedetached" or
            "expecteditable" or "tobeeditable" or "expectempty" or "tobeempty" or
            "expectfocused" or "tobefocused" or "expectinviewport" or "tobeinviewport" or
            "expectvalue" or "tohavevalue" or "expectvalues" or "tohavevalues" or "expectattribute" or "tohaveattribute" or
            "expectclass" or "tohaveclass" or "expectid" or "tohaveid" or
            "expectcss" or "tohavecss" or "expectproperty" or "tohavejsproperty" or
            "expectaccessiblename" or "tohaveaccessiblename" or "expectrole" or "tohaverole" or
            "expectchecked" or "tobechecked" or "expectcount" or "tohavecount" => ExecuteElementExpectation(remoteDebuggingUrl, automationClient, action),
            "evaluate" => ExecuteEvaluate(remoteDebuggingUrl, automationClient, action),
            "expecteval" or "asserteval" or "expectexpression" or "assertexpression" =>
                ExecuteEvaluateAssertion(remoteDebuggingUrl, automationClient, action),
            "evaluateonselector" or "evalonselector" or "evaluateall" or "evalall" => ExecuteSelectorEvaluate(remoteDebuggingUrl, automationClient, action),
            "url" or "title" or "content" or "setcontent" => ExecutePageContentAction(remoteDebuggingUrl, automationClient, action),
            "addinitscript" or "evaluateonnewdocument" => ExecuteAddInitScript(remoteDebuggingUrl, automationClient, action),
            "addscripttag" or "addstyletag" => ExecuteAddTag(remoteDebuggingUrl, automationClient, action),
            "exposefunction" or "exposebinding" => ExecuteExposeFunction(remoteDebuggingUrl, automationClient, action),
            "setviewport" or "viewport" or "setviewportsize" => ExecuteSetViewport(remoteDebuggingUrl, automationClient, action),
            "apirequest" => ExecuteApiRequest(action),
            "storagestate" => ExecuteStorageState(remoteDebuggingUrl, automationClient, action),
            "localstorage" or "sessionstorage" or "cookie" => ExecuteStorageAction(remoteDebuggingUrl, automationClient, action),
            "expectscreenshot" or "tohavescreenshot" => ExecuteVisualAssertion(remoteDebuggingUrl, automationClient, action),
            "uploadfiles" or "setinputfiles" or "selectfile" => ExecuteUploadFiles(remoteDebuggingUrl, automationClient, action with { Name = "uploadFiles" }),
            "emulate" => ExecuteEmulate(remoteDebuggingUrl, automationClient, action),
            "setgeolocation" or "grantpermissions" or "clearpermissions" or
            "setjavascriptenabled" or "javascriptenabled" or "bypasscsp" or "serviceworkers" or "setserviceworkers" =>
                ExecuteGeolocationOrPermission(remoteDebuggingUrl, automationClient, action),
            "download" => ExecuteDownload(remoteDebuggingUrl, automationClient, action),
            "waitfordownload" => ExecuteWaitForDownload(action),
            "captureconsole" => ExecuteCaptureConsole(remoteDebuggingUrl, automationClient, action),
            "waitforconsole" => ExecuteWaitForConsole(remoteDebuggingUrl, automationClient, action),
            "expectnoconsole" or "tohavenoconsole" => ExecuteExpectNoConsole(remoteDebuggingUrl, automationClient, action),
            "capturedialogs" or "setdialogbehavior" or "ondialog" or "handledialog" or "dialogbehavior" or "waitfordialog" =>
                ExecuteDialogAction(remoteDebuggingUrl, automationClient, action),
            "waitforevent" => ExecuteWaitForEvent(remoteDebuggingUrl, automationClient, action),
            "capturepageerrors" => ExecuteCapturePageErrors(remoteDebuggingUrl, automationClient, action),
            "waitforpageerror" => ExecuteWaitForPageError(remoteDebuggingUrl, automationClient, action),
            "route" or "mockresponse" or "intercept" => ExecuteRoute(remoteDebuggingUrl, automationClient, action),
            "clearroutes" => ExecuteClearRoutes(remoteDebuggingUrl, automationClient, action),
            "waitforrequest" => ExecuteWaitForRequest(remoteDebuggingUrl, automationClient, action),
            "waitforrequestfinished" => ExecuteWaitForRequestFinished(remoteDebuggingUrl, automationClient, action),
            "waitforrequestfailed" => ExecuteWaitForRequestFailed(remoteDebuggingUrl, automationClient, action),
            "waitforresponse" => ExecuteWaitForResponse(remoteDebuggingUrl, automationClient, action),
            "exporthar" => ExecuteExportHar(remoteDebuggingUrl, automationClient, action),
            "replayhar" => ExecuteReplayHar(remoteDebuggingUrl, automationClient, action),
            "routewebsocket" or "clearwebsocketroutes" or "waitforwebsocket" or "waitforwebsocketmessage" => ExecuteWebSocketAction(remoteDebuggingUrl, automationClient, action),
            "setextrahttpheaders" or "setheaders" or "clearextrahttpheaders" or "clearheaders" or
            "sethttpcredentials" or "httpcredentials" or "authenticate" or "clearhttpcredentials" or
            "setproxy" or "proxy" or "clearproxy" or "setoffline" => ExecuteNetworkEnvironmentAction(remoteDebuggingUrl, automationClient, action),
            "frameclick" or "frametype" or "framefill" or "framehover" or
            "framewaitforelement" or "frameasserttext" or "frameevaluate" => ExecuteFrameAction(remoteDebuggingUrl, automationClient, action),
            "clock" or "tick" or "restoreclock" => ExecuteClockAction(remoteDebuggingUrl, automationClient, action),
            "clearcontext" or "resetcontext" => ExecuteContextAction(remoteDebuggingUrl, automationClient, action),
            "newcontext" or "usecontext" or "listcontexts" or "closecontext" => ExecuteBrowserContextAction(remoteDebuggingUrl, automationClient, action, context),
            "listworkers" or "waitforworker" or "workerevaluate" or "workerintercept" => ExecuteWorkerAction(remoteDebuggingUrl, automationClient, action),
            "startcoverage" or "stopcoverage" => ExecuteCoverageAction(remoteDebuggingUrl, automationClient, action),
            "accessibilitysnapshot" => ExecuteAccessibilitySnapshot(remoteDebuggingUrl, automationClient, action),
            "expectaccessible" => ExecuteExpectAccessible(remoteDebuggingUrl, automationClient, action),
            "movemouse" => ExecuteMoveMouse(action, recorder, dragging: false),
            "mousemove" or "mousedown" or "mouseup" => ExecuteMouseAction(remoteDebuggingUrl, automationClient, action, recorder),
            "scrollto" or "scrollby" or "wheel" => ExecuteScrollAction(remoteDebuggingUrl, automationClient, action, recorder),
            "draganddrop" or "dragto" => ExecuteDragAndDrop(remoteDebuggingUrl, automationClient, action with { Name = "dragAndDrop" }, recorder),
            "gif" or "recordvideo" or "screencast" => ExecuteGifBlock(remoteDebuggingUrl, automationClient, action, context, recorder),
            "listtabs" => ExecuteListTabs(remoteDebuggingUrl, automationClient, action),
            "opentab" => ExecuteOpenTab(remoteDebuggingUrl, automationClient, action),
            "waitfortab" or "waitforpopup" => ExecuteWaitForTab(remoteDebuggingUrl, automationClient, action),
            "activatetab" => ExecuteActivateTab(remoteDebuggingUrl, automationClient, action),
            "closetab" => ExecuteCloseTab(remoteDebuggingUrl, automationClient, action),
            "readfile" or "fixture" or "writefile" or "appendfile" or "expectfile" => ExecuteFileAction(action, context),
            "set" => ExecuteSet(remoteDebuggingUrl, automationClient, action, context, recorder),
            "macro" or "call" or "return" or "if" or "elseif" or "else" or "switch" or "case" or "default" or
            "for" or "foreach" or "foreachselector" or "while" or "until" or "dowhile" or "dountil" or "repeat" or "retry" or "break" or "continue" or "try" or "catch" or "finally" =>
                ExecuteControlAction(remoteDebuggingUrl, automationClient, action, context, recorder),
            _ => throw new ScriptExecutionException($"Unknown action '{action.Name}'.")
        };
    }
}
