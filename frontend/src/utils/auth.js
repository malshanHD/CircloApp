const SESSION_EVENT = "circlo-session";
export function readSession() {
  const token = localStorage.getItem("accessToken");
  if (!token) return null;
  try {
    const encoded = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");
    const claims = JSON.parse(
      atob(encoded.padEnd(Math.ceil(encoded.length / 4) * 4, "=")),
    );
    if (!Number.isFinite(claims.exp) || claims.exp * 1000 <= Date.now())
      return null;
    // Display hints only; API authorization remains authoritative.
    return {
      token,
      expiresAt: claims.exp * 1000,
      username: claims.unique_name || "friend",
      email: claims.email || "",
    };
  } catch {
    return null;
  }
}
export const auth = {
  getToken: () => readSession()?.token,
  isAuthenticated: () => Boolean(readSession()),
  login: (response) => {
    localStorage.setItem("accessToken", response.accessToken);
    window.dispatchEvent(new Event(SESSION_EVENT));
  },
  logout: () => {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    window.dispatchEvent(new Event(SESSION_EVENT));
  },
};
export { SESSION_EVENT };
