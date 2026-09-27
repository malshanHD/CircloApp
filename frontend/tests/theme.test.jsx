import { afterEach, expect, it, vi } from "vitest";
import { cleanup, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import ThemeProvider from "../src/context/ThemeContext";
import ThemeToggle from "../src/components/ThemeToggle";

afterEach(() => {
  cleanup();
  localStorage.removeItem("circlo-theme");
  delete document.documentElement.dataset.theme;
  vi.restoreAllMocks();
});
it("toggles by keyboard and restores the saved choice after remount", async () => {
  localStorage.setItem("circlo-theme", "light");
  const user = userEvent.setup();
  const view = render(<ThemeProvider><ThemeToggle /></ThemeProvider>);
  await user.tab();
  await user.keyboard("{Enter}");
  expect(document.documentElement.dataset.theme).toBe("dark");
  expect(localStorage.getItem("circlo-theme")).toBe("dark");
  view.unmount();
  render(<ThemeProvider><ThemeToggle /></ThemeProvider>);
  await user.click(screen.getByRole("button", { name: "Switch to light mode" }));
  expect(document.documentElement.dataset.theme).toBe("light");
});
it("follows system dark mode and still toggles when storage is unavailable", async () => {
  vi.spyOn(Storage.prototype, "getItem").mockImplementation(() => { throw new Error("Blocked"); });
  vi.spyOn(Storage.prototype, "setItem").mockImplementation(() => { throw new Error("Blocked"); });
  vi.spyOn(window, "matchMedia").mockReturnValue({ matches: true, addEventListener: vi.fn(), removeEventListener: vi.fn() });
  render(<ThemeProvider><ThemeToggle /></ThemeProvider>);
  expect(document.documentElement.dataset.theme).toBe("dark");
  await userEvent.click(screen.getByRole("button", { name: "Switch to light mode" }));
  expect(document.documentElement.dataset.theme).toBe("light");
});
