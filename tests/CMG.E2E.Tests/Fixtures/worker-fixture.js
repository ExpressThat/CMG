self.__cmgWorkerState = {
  startedAt: Date.now(),
  messages: []
};

self.addEventListener("message", event => {
  self.__cmgWorkerState.messages.push(event.data);
  self.postMessage({ kind: "echo", value: event.data });
});
