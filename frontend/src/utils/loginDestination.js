export function loginDestination(requested) {
  if (typeof requested !== "string" || !requested.startsWith("/") || requested.startsWith("//")) return "/dashboard";
  try {
    const url = new URL(requested, window.location.origin);
    return url.origin === window.location.origin && !["/login", "/register", "/verify-otp"].includes(url.pathname)
      ? url.pathname + url.search + url.hash : "/dashboard";
  } catch { return "/dashboard"; }
}

