import { createContext, useEffect, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { auth, readSession, SESSION_EVENT } from "../utils/auth";
export const AuthContext = createContext(null);
export default function AuthProvider({ children }) {
  const [session, setSession] = useState(readSession);
  const client = useQueryClient();
  useEffect(() => {
    const sync = () => {
      client.clear();
      setSession(readSession());
    };
    window.addEventListener(SESSION_EVENT, sync);
    window.addEventListener("storage", sync);
    return () => {
      window.removeEventListener(SESSION_EVENT, sync);
      window.removeEventListener("storage", sync);
    };
  }, [client]);
  useEffect(() => {
    if (!session) return;
    const timer = setTimeout(
      auth.logout,
      Math.max(0, session.expiresAt - Date.now()),
    );
    return () => clearTimeout(timer);
  }, [session]);
  return (
    <AuthContext.Provider
      value={{ session, login: auth.login, logout: auth.logout }}
    >
      {children}
    </AuthContext.Provider>
  );
}
