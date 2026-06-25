using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public static partial class BrowserDomScripts
{
    public static string AutoAcceptDialogs() =>
        """
        (() => {
          if (window.__cmg_auto_accept_dialogs_installed) return true;
          window.__cmg_auto_accept_dialogs_installed = true;
          window.alert = () => {};
          window.confirm = () => true;
          window.prompt = (_message, defaultValue = '') => defaultValue ?? '';
          try {
            Object.defineProperty(window, 'onbeforeunload', {
              configurable: true,
              get: () => null,
              set: () => {}
            });
          } catch {}
          const originalAddEventListener = EventTarget.prototype.addEventListener;
          EventTarget.prototype.addEventListener = function(type, listener, options) {
            if (this === window && String(type).toLowerCase() === 'beforeunload') {
              return;
            }

            return originalAddEventListener.call(this, type, listener, options);
          };
          return true;
        })()
        """;

    public static string AutoAcceptDialogsPreload() =>
        """
        () => {
          if (window.__cmg_auto_accept_dialogs_installed) return;
          window.__cmg_auto_accept_dialogs_installed = true;
          window.alert = () => {};
          window.confirm = () => true;
          window.prompt = (_message, defaultValue = '') => defaultValue ?? '';
          try {
            Object.defineProperty(window, 'onbeforeunload', {
              configurable: true,
              get: () => null,
              set: () => {}
            });
          } catch {}
          const originalAddEventListener = EventTarget.prototype.addEventListener;
          EventTarget.prototype.addEventListener = function(type, listener, options) {
            if (this === window && String(type).toLowerCase() === 'beforeunload') {
              return;
            }

            return originalAddEventListener.call(this, type, listener, options);
          };
        }
        """;
}
