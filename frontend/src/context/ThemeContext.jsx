import { createContext, useContext, useEffect, useState } from "react";

const ThemeContext = createContext(null);
const storageKey = "circlo-theme";
function savedTheme() {
  try {
    const value = localStorage.getItem(storageKey);
    return value === "dark" || value === "light" ? value : null;
  } catch { return null; }
}
function systemTheme() {
  return window.matchMedia?.("(prefers-color-scheme: dark)").matches ? "dark" : "light";
}
export default function ThemeProvider({ children }) {
  const [preference, setPreference] = useState(savedTheme);
  const [system, setSystem] = useState(systemTheme);
  const theme = preference || system;
  useEffect(() => {
    const media = window.matchMedia("(prefers-color-scheme: dark)");
    const onChange = event => setSystem(event.matches ? "dark" : "light");
    const onStorage = event => {
      if (event.key === storageKey || event.key === null) setPreference(savedTheme());
    };
    media.addEventListener("change", onChange);
    window.addEventListener("storage", onStorage);
    return () => {
      media.removeEventListener("change", onChange);
      window.removeEventListener("storage", onStorage);
    };
  }, []);
  useEffect(() => {
    document.documentElement.dataset.theme = theme;
    document.documentElement.style.colorScheme = theme;
  }, [theme]);
  function toggleTheme() {
    const next = theme === "dark" ? "light" : "dark";
    setPreference(next);
    try { localStorage.setItem(storageKey, next); } catch { /* Still works without storage. */ }
  }
  return <ThemeContext.Provider value={{ theme, toggleTheme }}>{children}</ThemeContext.Provider>;
}
export function useTheme() {
  return useContext(ThemeContext);
}
