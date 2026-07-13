using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Runner;

public sealed partial class CmgRunService
{
    public CmgRunService(
        BrowserStateStore stateStore,
        IBrowserController browserController,
        BrowserAutomationClientFactory automationClientFactory,
        BrowserScriptRunner scriptRunner,
        CmgDslParser parser,
        CmgTestPlanner planner,
        CmgActionLowerer lowerer,
        CmgValidator validator,
        CmgApiRequestRunner apiRequestRunner,
        CmgStorageStateRunner storageStateRunner,
        CmgVisualAssertionRunner visualAssertionRunner,
        CmgUploadRunner uploadRunner)
        : this(stateStore, browserController, new BrowserLeaseManager(stateStore, browserController, new BrowserLeaseMonitorLauncher()),
            automationClientFactory, scriptRunner, parser, planner, lowerer, validator, apiRequestRunner,
            storageStateRunner, visualAssertionRunner, uploadRunner)
    {
    }
}
