import { afterEach, expect, it, vi } from "vitest";
import api from "../src/services/api";
import { waitForApi } from "../src/services/apiReadiness";
afterEach(() => { vi.restoreAllMocks(); vi.useRealTimers(); });
it("polls every 2.5 seconds and resolves only when healthy", async () => {
  vi.useFakeTimers();
  const get = vi.spyOn(api, "get").mockRejectedValueOnce({ response: { status: 503 } })
    .mockResolvedValueOnce({ status: 200, data: { status: "Healthy" } });
  const waiting = vi.fn();
  const pending = waitForApi({ onWaiting: waiting });
  await vi.advanceTimersByTimeAsync(2499);
  expect(get).toHaveBeenCalledTimes(1);
  await vi.advanceTimersByTimeAsync(1);
  await pending;
  expect(get).toHaveBeenCalledTimes(2);
  expect(get.mock.calls[0][1].headers.Authorization).toBe(null);
  expect(get.mock.calls[0][1].baseURL.endsWith("/api")).toBe(false);
  expect(waiting).toHaveBeenCalledTimes(1);
});
it("aborts pending retries on navigation", async () => {
  vi.useFakeTimers();
  const get = vi.spyOn(api, "get").mockRejectedValue(new Error("Network"));
  const controller = new AbortController();
  const pending = waitForApi({ signal: controller.signal });
  const rejected = expect(pending).rejects.toThrow();
  await vi.advanceTimersByTimeAsync(0);
  controller.abort();
  await rejected;
  await vi.advanceTimersByTimeAsync(5000);
  expect(get).toHaveBeenCalledTimes(1);
});
it("bounds unavailable-server waits and does not retry configuration errors", async () => {
  vi.useFakeTimers();
  const get = vi.spyOn(api, "get").mockRejectedValue({ response: { status: 503 } });
  const pending = expect(waitForApi({ maxWaitMs: 5000 })).rejects.toThrow("taking longer");
  await vi.advanceTimersByTimeAsync(5000);
  await pending;
  expect(get).toHaveBeenCalledTimes(2);
  get.mockRejectedValue({ response: { status: 404 } });
  await expect(waitForApi()).rejects.toThrow("readiness check");
});
