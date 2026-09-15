import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  cleanup,
  fireEvent,
  render,
  screen,
  waitFor,
} from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import App from "../src/App";
import api from "../src/services/api";
import { auth, readSession } from "../src/utils/auth";
import { getApiError } from "../src/utils/apiError";
import { eventId, fixture, token, resetFixtureState } from "./fixtures";
let requests;
function mount(path = "/login", loggedIn = false) {
  if (loggedIn) localStorage.setItem("accessToken", token());
  window.history.replaceState({}, "", path);
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });
  render(
    <QueryClientProvider client={client}>
      <App />
    </QueryClientProvider>,
  );
  return client;
}
beforeEach(() => {
  requests = [];
  resetFixtureState();
  localStorage.clear();
  sessionStorage.clear();
  api.defaults.adapter = async (config) => {
    const data =
      typeof config.data === "string" ? JSON.parse(config.data) : config.data;
    requests.push({
      url: config.url,
      method: config.method,
      data,
      authorization: config.headers.Authorization,
    });
    const result = fixture(config.method.toUpperCase(), config.url, data);
    const response = {
      data: result.body,
      status: result.status || 200,
      config,
      statusText: "",
      headers: {},
    };
    if (response.status >= 400)
      throw Object.assign(new Error("Request failed"), {
        response,
        config,
        isAxiosError: true,
      });
    return response;
  };
});
afterEach(() => {
  cleanup();
  vi.restoreAllMocks();
});
describe("authentication and errors", () => {
  it("protects a deep link, focuses invalid fields, submits current values and restores the destination", async () => {
    mount(`/events/${eventId}?tab=ai`);
    const user = userEvent.setup();
    await user.click(
      await screen.findByRole("button", { name: "Sign in", exact: true }),
    );
    expect(document.activeElement).toBe(
      screen.getByLabelText("Email or username"),
    );
    await user.type(
      screen.getByLabelText("Email or username"),
      "jamie@example.test",
    );
    await user.type(
      screen.getByLabelText("Password", { exact: true }),
      "correct-password",
    );
    await user.click(
      screen.getByRole("button", { name: "Sign in", exact: true }),
    );
    await screen.findByRole("heading", { name: "Weekend in the hills" });
    expect(window.location.search).toBe("?tab=ai");
    expect(requests.find((r) => r.url === "/auth/login").data).toEqual({
      usernameOrEmail: "jamie@example.test",
      password: "correct-password",
    });
    expect(
      requests.find((r) => r.url === `/events/${eventId}`).authorization,
    ).toMatch(/^Bearer test\./);
    await user.click(screen.getAllByRole("button", { name: "Log out" })[0]);
    await screen.findByRole("heading", { name: "Welcome back." });
    expect(localStorage.getItem("accessToken")).toBeNull();
  });
  it("shows the exact login failure and preserves input", async () => {
    mount();
    const user = userEvent.setup();
    await user.type(screen.getByLabelText("Email or username"), "jamie");
    await user.type(
      screen.getByLabelText("Password", { exact: true }),
      "wrong",
    );
    await user.click(
      screen.getByRole("button", { name: "Sign in", exact: true }),
    );
    await screen.findByText("Invalid username/email or password.");
    expect(screen.getByLabelText("Email or username").value).toBe("jamie");
  });
  it("registers with the exact DTO, verifies pasted OTP and navigates to login", async () => {
    mount("/register");
    const user = userEvent.setup();
    for (const [label, value] of [
      ["First name", "Jamie"],
      ["Last name", "Taylor"],
      ["Email address", "jamie@example.test"],
      ["Username", "jamie"],
      ["Password", "password123"],
      ["Confirm password", "password123"],
    ])
      await user.type(screen.getByLabelText(label, { exact: true }), value);
    await user.click(
      screen.getByRole("button", { name: "Create account", exact: true }),
    );
    const digit = await screen.findByLabelText("Digit 1");
    expect(requests.find((r) => r.url === "/auth/register").data).toEqual({
      firstName: "Jamie",
      lastName: "Taylor",
      email: "jamie@example.test",
      username: "jamie",
      contactNumber: "",
      password: "password123",
    });
    fireEvent.paste(digit, { clipboardData: { getData: () => "123456" } });
    expect(screen.getByLabelText("Digit 6").value).toBe("6");
    await user.click(screen.getByRole("button", { name: "Verify email" }));
    await screen.findByText(/Email verified. Taking/);
    await screen.findByRole(
      "heading",
      { name: "Welcome back." },
      { timeout: 4000 },
    );
    expect(sessionStorage.getItem("circlo-registration")).toBeNull();
  });
  it("shows duplicate registration messages without leaving the form", async () => {
    mount("/register");
    const user = userEvent.setup();
    for (const [label, value] of [
      ["First name", "Jamie"],
      ["Last name", "Taylor"],
      ["Email address", "duplicate@example.test"],
      ["Username", "jamie"],
      ["Password", "password123"],
      ["Confirm password", "password123"],
    ])
      fireEvent.change(screen.getByLabelText(label, { exact: true }), {
        target: { value },
      });
    await user.click(
      screen.getByRole("button", { name: "Create account", exact: true }),
    );
    await screen.findByText("User with the specified email already exists.");
    expect(window.location.pathname).toBe("/register");
  });
  it("handles invalid OTP, backspace and a persistent countdown without fake resend", async () => {
    sessionStorage.setItem(
      "circlo-registration",
      JSON.stringify({
        email: "jamie@example.test",
        expiresAt: Date.now() + 300000,
      }),
    );
    mount("/verify-otp");
    const user = userEvent.setup();
    fireEvent.paste(screen.getByLabelText("Digit 1"), {
      clipboardData: { getData: () => "111111" },
    });
    await user.click(screen.getByRole("button", { name: "Verify email" }));
    await screen.findByText("Invalid OTP");
    fireEvent.change(screen.getByLabelText("Digit 2"), {
      target: { value: "" },
    });
    screen.getByLabelText("Digit 2").focus();
    fireEvent.keyDown(screen.getByLabelText("Digit 2"), { key: "Backspace" });
    expect(document.activeElement).toBe(screen.getByLabelText("Digit 1"));
    expect(screen.queryByRole("button", { name: /Resend/ })).toBeNull();
    expect(screen.getByText(/Code window/)).toBeTruthy();
  });
  it("rejects malformed and expired sessions and maps all error shapes", () => {
    localStorage.setItem("accessToken", "invalid");
    expect(readSession()).toBeNull();
    localStorage.setItem(
      "accessToken",
      `test.${btoa(JSON.stringify({ exp: 1 }))}.test`,
    );
    expect(auth.isAuthenticated()).toBe(false);
    expect(
      getApiError({
        response: {
          status: 400,
          data: {
            message: "Validation failed.",
            errors: { Name: ["Name required."] },
          },
        },
      }),
    ).toEqual({ message: "Validation failed.", errors: ["Name required."] });
    for (const status of [400, 401, 403, 404, 500])
      expect(getApiError({ response: { status } }).message).toBeTruthy();
    expect(getApiError({ isAxiosError: true }).message).toContain("connection");
  });
  it("logs out on concurrent unauthorized responses without refresh requests", async () => {
    mount("/events", true);
    await screen.findByRole("heading", { name: "Your plans. Your people." });
    api.defaults.adapter = async (config) => {
      throw { response: { status: 401 }, config };
    };
    await Promise.allSettled([api.get("/events"), api.get("/expenses")]);
    await screen.findByRole("heading", { name: "Welcome back." });
    expect(localStorage.getItem("accessToken")).toBeNull();
    expect(requests.some((r) => /refresh/.test(r.url))).toBe(false);
  });
});
describe("events and AI", () => {
  it("loads events, filters them, validates creation, invalidates cache and opens the new event", async () => {
    const client = mount("/events", true);
    const user = userEvent.setup();
    await screen.findByRole("heading", { name: "Weekend in the hills" });
    await user.type(
      screen.getByPlaceholderText("Search this page…"),
      "no-match",
    );
    await screen.findByText("No matching plans on this page.");
    await user.click(screen.getByRole("button", { name: "Clear search" }));
    await user.click(screen.getByRole("button", { name: "New event" }));
    await user.click(
      screen
        .getAllByRole("button", { name: "Create event", exact: true })
        .at(-1),
    );
    await screen.findByText("Give your event a name.");
    expect(document.activeElement).toBe(screen.getByLabelText("Event name"));
    await user.type(screen.getByLabelText("Event name"), "Invalid");
    await user.type(
      screen.getByLabelText("Description", { exact: true }),
      "A test description",
    );
    await user.click(
      screen
        .getAllByRole("button", { name: "Create event", exact: true })
        .at(-1),
    );
    await screen.findByText("Name is not valid.");
    await user.clear(screen.getByLabelText("Event name"));
    await user.type(screen.getByLabelText("Event name"), "New test event");
    const invalidation = vi.spyOn(client, "invalidateQueries");
    await user.click(
      screen
        .getAllByRole("button", { name: "Create event", exact: true })
        .at(-1),
    );
    await screen.findByRole("heading", { name: "New test event" });
    expect(invalidation).toHaveBeenCalledWith({ queryKey: ["events"] });
  });
  it("shows event loading errors with retry and rejects invalid route IDs", async () => {
    api.defaults.adapter = async (config) => {
      throw {
        response: { status: 500, data: { message: "Server unavailable." } },
        config,
      };
    };
    mount("/events", true);
    await screen.findByText("Server unavailable.");
    expect(screen.getByRole("button", { name: "Try again" })).toBeTruthy();
    cleanup();
    mount("/events/not-a-guid", true);
    await screen.findByText("This event link is invalid.");
  });
  it("sends event-scoped questions, supports Shift+Enter, displays errors and retries", async () => {
    mount(`/events/${eventId}?tab=ai`, true);
    const user = userEvent.setup();
    const input = await screen.findByLabelText("Ask about this event");
    await user.type(input, "Summarize");
    await user.keyboard("{Shift>}{Enter}{/Shift}");
    expect(input.value).toContain("\n");
    await user.keyboard("{Enter}");
    await screen.findByText(/For this test event, recorded expenses/);
    expect(requests.find((r) => r.url === "/Ask/ask").data).toEqual({
      eventId,
      question: "Summarize",
    });
    await user.type(input, "fail");
    await user.keyboard("{Enter}");
    await screen.findByText("The AI service is temporarily unavailable.");
    await user.click(screen.getByRole("button", { name: "Try again" }));
    await waitFor(() =>
      expect(requests.filter((r) => r.url === "/Ask/ask")).toHaveLength(3),
    );
    expect(screen.getAllByText("fail", { exact: true })).toHaveLength(1);
  });
  it("prevents duplicate AI sends while a request is pending", async () => {
    mount(`/events/${eventId}?tab=ai`, true);
    const user = userEvent.setup();
    const input = await screen.findByLabelText("Ask about this event");
    const adapter = api.defaults.adapter;
    let resolve;
    api.defaults.adapter = (config) =>
      config.url === "/Ask/ask"
        ? new Promise((done) => {
            resolve = () =>
              done({
                data: { answer: "Done" },
                status: 200,
                config,
                headers: {},
              });
          })
        : adapter(config);
    await user.type(input, "A question");
    await user.keyboard("{Enter}{Enter}");
    await screen.findByText("Circlo is thinking…");
    expect(screen.getByRole("button", { name: "Send question" }).disabled).toBe(
      true,
    );
    resolve();
    await screen.findByText("Done", { exact: true });
  });
});
