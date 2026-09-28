import { FiUser, FiUsers, FiGrid, FiBarChart2, FiSliders, FiCalendar, FiZap } from "react-icons/fi";

export const MODE_KEY = "circlo-workspace-mode";
export const workspaces = {
  personal: {
    label: "Personal", icon: FiUser, home: "/personal-expenses", action: "Add expense",
    subtitle: "My money", copy: "Your money, a little clearer.",
    description: "Small details today. A clearer picture tomorrow.",
    navigation: [
      { to: "/personal-expenses", label: "Overview & expenses", icon: FiGrid },
      { to: "/personal-expenses/analysis", label: "Insights", icon: FiBarChart2 },
      { to: "/personal-expenses/settings", label: "Budget settings", icon: FiSliders },
    ],
  },
  groups: {
    label: "Groups", icon: FiUsers, home: "/dashboard", action: "Create event",
    subtitle: "Shared plans", copy: "Shared plans, clear expenses.",
    description: "Bring your people together. Keep the details simple.",
    navigation: [
      { to: "/dashboard", label: "Group overview", icon: FiGrid },
      { to: "/events", label: "My events", icon: FiCalendar },
      { to: "/assistant", label: "Circlo AI", icon: FiZap },
    ],
  },
};
export function readPreference(key) {
  try { return localStorage.getItem(key); } catch { return null; }
}
export function savePreference(key, value) {
  try { localStorage.setItem(key, value); } catch { /* Navigation also works without storage. */ }
}
export function storedMode() {
  return readPreference(MODE_KEY) === "personal" ? "personal" : "groups";
}
export function routeMode(pathname) {
  if (/^\/personal-expenses(?:\/(?:analysis|settings))?\/?$/.test(pathname)) return "personal";
  if (/^\/(?:dashboard|assistant|accept-invite|events(?:\/[a-zA-Z0-9-]+)?)\/?$/.test(pathname)) return "groups";
  return null;
}
export function safeWorkspaceRoute(value, mode) {
  if (typeof value !== "string" || !value.startsWith("/") || /[\\\s]/.test(value) || value.startsWith("//")) return null;
  try {
    const url = new URL(value, "https://circlo.invalid");
    if (url.origin !== "https://circlo.invalid" || routeMode(url.pathname) !== mode) return null;
    // Invitation links may submit join requests; never replay them from route memory.
    if (url.pathname === "/accept-invite") return null;
    return url.pathname + url.search + url.hash;
  } catch { return null; }
}
export function lastWorkspaceRoute(mode) {
  return safeWorkspaceRoute(readPreference(`circlo-workspace-last-route-${mode}`), mode) || workspaces[mode].home;
}
export function workspaceSection(pathname, mode) {
  return workspaces[mode].navigation.find(item => item.to === pathname)?.label
    || (pathname.startsWith("/events/") ? "Event details" : pathname === "/accept-invite" ? "Event invitation" : "Overview");
}
