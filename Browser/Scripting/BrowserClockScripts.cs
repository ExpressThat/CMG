namespace CMG.Browser.Scripting;

public static class BrowserClockScripts
{
    public static string Install(long now) =>
        $$"""
        (() => {
          const original = window.__cmgClock?.original || {
            Date: window.Date,
            setTimeout: window.setTimeout.bind(window),
            clearTimeout: window.clearTimeout.bind(window),
            setInterval: window.setInterval.bind(window),
            clearInterval: window.clearInterval.bind(window)
          };
          let current = {{now}};
          let nextId = 1;
          const timers = new Map();
          function FakeDate(...args) {
            return args.length ? new original.Date(...args) : new original.Date(current);
          }
          FakeDate.now = () => current;
          FakeDate.UTC = original.Date.UTC;
          FakeDate.parse = original.Date.parse;
          FakeDate.prototype = original.Date.prototype;
          window.Date = FakeDate;
          window.setTimeout = (callback, delay = 0, ...args) => schedule(false, callback, delay, args);
          window.setInterval = (callback, delay = 0, ...args) => schedule(true, callback, delay, args);
          window.clearTimeout = id => timers.delete(id);
          window.clearInterval = id => timers.delete(id);
          function schedule(interval, callback, delay, args) {
            const id = nextId++;
            timers.set(id, { interval, callback, delay: Number(delay) || 0, due: current + (Number(delay) || 0), args });
            return id;
          }
          window.__cmgClock = { original, timers, get current() { return current; }, set current(value) { current = value; } };
          return current;
        })()
        """;

    public static string Tick(long milliseconds) =>
        $$"""
        (() => {
          const clock = window.__cmgClock;
          if (!clock) throw new Error('Clock is not installed. Run clock now=<epoch-ms> first.');
          const end = clock.current + {{milliseconds}};
          while (true) {
            const due = [...clock.timers.entries()].filter(item => item[1].due <= end).sort((a, b) => a[1].due - b[1].due)[0];
            if (!due) break;
            const [id, timer] = due;
            clock.current = timer.due;
            if (!timer.interval) clock.timers.delete(id);
            timer.callback(...timer.args);
            if (timer.interval && clock.timers.has(id)) timer.due = clock.current + timer.delay;
          }
          clock.current = end;
          return clock.current;
        })()
        """;

    public static string Restore() =>
        """
        (() => {
          const clock = window.__cmgClock;
          if (!clock) return false;
          window.Date = clock.original.Date;
          window.setTimeout = clock.original.setTimeout;
          window.clearTimeout = clock.original.clearTimeout;
          window.setInterval = clock.original.setInterval;
          window.clearInterval = clock.original.clearInterval;
          delete window.__cmgClock;
          return true;
        })()
        """;
}
