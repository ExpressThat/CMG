namespace CMG.Browser.Scripting;

public static class BrowserContextScripts
{
    public static string Clear(bool navigateBlank) =>
        $$"""
        (async () => {
          localStorage.clear();
          sessionStorage.clear();
          for (const cookie of document.cookie.split(';')) {
            const name = cookie.split('=')[0]?.trim();
            if (name) {
              document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/`;
            }
          }
          if ('indexedDB' in window && indexedDB.databases) {
            for (const database of await indexedDB.databases()) {
              if (database.name) indexedDB.deleteDatabase(database.name);
            }
          }
          if ('caches' in window) {
            for (const key of await caches.keys()) await caches.delete(key);
          }
          if (navigator.serviceWorker) {
            for (const registration of await navigator.serviceWorker.getRegistrations()) await registration.unregister();
          }
          if ({{(navigateBlank ? "true" : "false")}}) location.href = 'about:blank';
          return true;
        })()
        """;
}
