import { createContext, useContext, useEffect } from "react";
import { useLocation, useNavigate, Navigate } from "react-router-dom";
import { MODE_KEY, workspaces, routeMode, storedMode, savePreference, safeWorkspaceRoute, lastWorkspaceRoute } from "./config";

const WorkspaceContext = createContext(null);
export function WorkspaceProvider({ children }) {
  const location = useLocation();
  const navigate = useNavigate();
  // The route is authoritative, including refresh and browser back/forward.
  const mode = routeMode(location.pathname) || storedMode();
  useEffect(() => {
    savePreference(MODE_KEY, mode);
    const route = safeWorkspaceRoute(location.pathname + location.search + location.hash, mode);
    if (route) savePreference(`circlo-workspace-last-route-${mode}`, route);
  }, [mode, location.pathname, location.search, location.hash]);
  function switchMode(next) {
    if (!Object.hasOwn(workspaces, next) || next === mode) return;
    savePreference(MODE_KEY, next);
    navigate(lastWorkspaceRoute(next));
  }
  return <WorkspaceContext.Provider value={{ mode, config: workspaces[mode], switchMode }}>{children}</WorkspaceContext.Provider>;
}
export function useWorkspace() { return useContext(WorkspaceContext); }
export function WorkspaceLanding() {
  return <Navigate to={lastWorkspaceRoute(storedMode())} replace />;
}
