import api from "./api";

// Probe the API host, not /api/health. Do not send credentials or a bearer token.
export async function waitForApi({ signal, onWaiting = () => {}, maxWaitMs = 120000 } = {}) {
  const configured = import.meta.env.VITE_API_BASE_URL;
  if (!configured) throw new Error("Circlo's API URL is not configured. Set VITE_API_BASE_URL.");
  const baseURL = new URL(configured, window.location.origin).origin;
  const deadline = Date.now() + maxWaitMs;
  while (Date.now() < deadline) {
    signal?.throwIfAborted();
    const started = Date.now();
    try {
      const response = await api.get("/health", {
        baseURL, signal, timeout: Math.min(2000, deadline - started),
        headers: { Authorization: null },
      });
      if (response.status === 200 && response.data?.status === "Healthy") return;
    } catch (error) {
      if (signal?.aborted) throw error;
      const status = error.response?.status;
      if (status && status < 500 && status !== 408 && status !== 429) {
        throw new Error("Circlo's readiness check is unavailable. Please try again later.", { cause: error });
      }
    }
    onWaiting();
    const delay = Math.min(Math.max(0, 2500 - (Date.now() - started)), deadline - Date.now());
    if (delay > 0) await new Promise((resolve, reject) => {
      const abort = () => { clearTimeout(timer); reject(signal.reason); };
      const timer = setTimeout(() => { signal?.removeEventListener("abort", abort); resolve(); }, delay);
      signal?.addEventListener("abort", abort, { once: true });
      if (signal?.aborted) abort();
    });
  }
  throw new Error("Circlo is taking longer than usual to connect. Please check your connection and try signing in again in a moment.");
}
