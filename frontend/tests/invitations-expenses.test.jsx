import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  cleanup,
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import App from "../src/App";
import api from "../src/services/api";
import {
  eventId,
  newEventId,
  userId,
  fixture,
  resetFixtureState,
  token,
} from "./fixtures";
let requests, override, client;
beforeEach(() => {
  resetFixtureState();
  localStorage.clear();
  sessionStorage.clear();
  requests = [];
  override = undefined;
  api.defaults.adapter = async (config) => {
    const data =
      typeof config.data === "string" ? JSON.parse(config.data) : config.data;
    requests.push({
      url: config.url,
      method: config.method,
      data,
      authorization: config.headers.Authorization,
    });
    const result = await (override?.(config) ??
      fixture(config.method.toUpperCase(), config.url, data));
    const response = {
      data: result.body,
      status: result.status || 200,
      headers: {},
      config,
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
  client?.clear();
  vi.restoreAllMocks();
});
function mount(path = "/events", authenticated = true) {
  if (authenticated) localStorage.setItem("accessToken", token());
  window.history.replaceState({}, "", path);
  client = new QueryClient({
    defaultOptions: {
      queries: { retry: false, retryDelay: 0 },
      mutations: { retry: false },
    },
  });
  render(
    <QueryClientProvider client={client}>
      <App />
    </QueryClientProvider>,
  );
}
describe("invitation notifications", () => {
  it("uses the server count, accepts the exact event, refreshes count and opens the enrolled event", async () => {
    mount();
    const user = userEvent.setup();
    await user.click(
      await screen.findByRole("button", { name: "Join requests, 2 pending" }),
    );
    const dialog = screen.getByRole("dialog");
    expect(within(dialog).getByText("Malshan Perera")).toBeTruthy();
    expect(within(dialog).getByText("New test event")).toBeTruthy();
    await user.click(
      within(dialog).getByRole("button", {
        name: "Approve Malshan Perera for New test event",
      }),
    );
    await screen.findByText("Malshan Perera has been approved.");
    expect(window.location.pathname).toBe(`/events/${newEventId}`);
    await screen.findByRole("button", { name: "Join requests, 1 pending" });
    const request = requests.find((r) => r.url.endsWith("/approve"));
    expect(request.url).toBe(
      `/events/${newEventId}/join-requests/${userId}/approve`,
    );
    expect(request.method).toBe("post");
    expect(request.authorization).toMatch(/^Bearer /);
    expect(
      requests.filter((r) => r.url === "/events/join-requests").length,
    ).toBeGreaterThan(1);
    expect(
      client
        .getQueriesData({ queryKey: ["events"] })
        .some(([, data]) => data?.items.some((item) => item.id === newEventId)),
    ).toBe(true);
  });
  it("shows acceptance failure inline, keeps the count, and prevents duplicate submissions", async () => {
    let release;
    override = (config) =>
      config.url.endsWith("/approve")
        ? new Promise((resolve) => {
            release = () =>
              resolve({
                status: 400,
                body: { message: "No invitation found for this event." },
              });
          })
        : undefined;
    mount();
    const user = userEvent.setup();
    await user.click(
      await screen.findByRole("button", { name: "Join requests, 2 pending" }),
    );
    const button = screen.getByRole("button", {
      name: "Approve Malshan Perera for New test event",
    });
    await user.dblClick(button);
    expect(button.disabled).toBe(true);
    expect(requests.filter((r) => r.url.endsWith("/approve"))).toHaveLength(1);
    release();
    await screen.findByText("No invitation found for this event.");
    expect(screen.getByText("2 requests waiting for approval.")).toBeTruthy();
    expect(window.location.pathname).toBe("/events");
  });
  it("shows empty and error states without presenting failures as zero invitations", async () => {
    override = (config) =>
      config.url === "/events/join-requests" ? { body: [] } : undefined;
    mount();
    const user = userEvent.setup();
    await user.click(
      await screen.findByRole("button", { name: "Join requests, 0 pending" }),
    );
    await screen.findByText("You're all caught up.");
    cleanup();
    client.clear();
    override = (config) =>
      config.url === "/events/join-requests"
        ? { status: 500, body: { message: "Invitations could not be loaded." } }
        : undefined;
    mount();
    await user.click(
      await screen.findByRole(
        "button",
        { name: "Join requests, unable to refresh" },
        { timeout: 4000 },
      ),
    );
    await screen.findByText("Invitations could not be loaded.");
    expect(screen.getByRole("button", { name: "Try again" })).toBeTruthy();
    expect(screen.queryByText("You're all caught up.")).toBeNull();
  });
});
describe("event expense ledger", () => {
  it("requests the route event, displays backend amounts and filters expenses versus settlements", async () => {
    mount(`/events/${eventId}?tab=expenses`);
    const user = userEvent.setup();
    await screen.findByRole("heading", {
      name: "Every expense, in one place.",
    });
    await screen.findByText("Train tickets");
    expect(
      requests.some(
        (r) =>
          r.url === `/expenses/${eventId}/all-event-expenses` &&
          r.authorization.startsWith("Bearer "),
      ),
    ).toBe(true);
    expect(screen.getByText("800.00")).toBeTruthy();
    await user.selectOptions(screen.getByLabelText("Transaction type"), "2");
    expect(screen.queryByText("Train tickets")).toBeNull();
    expect(screen.getByText("Settlement payment to Jamie")).toBeTruthy();
    await user.selectOptions(screen.getByLabelText("Transaction type"), "all");
    await user.type(screen.getByLabelText("Search event expenses"), "Alex");
    expect(screen.getByText("Group lunch")).toBeTruthy();
    expect(screen.queryByText("Train tickets")).toBeNull();
  });
  it("refreshes the ledger after adding an expense using the real POST contract", async () => {
    mount(`/events/${eventId}?tab=expenses`);
    const user = userEvent.setup();
    await screen.findByText("Train tickets");
    const invalidation = vi.spyOn(client, "invalidateQueries");
    await user.click(
      screen.getByRole("button", { name: "Add expense", exact: true }),
    );
    const dialog = screen.getByRole("dialog");
    fireEvent.change(within(dialog).getByLabelText("Amount"), {
      target: { value: "120" },
    });
    fireEvent.change(within(dialog).getByLabelText("Description"), {
      target: { value: "Coffee" },
    });
    await user.click(
      within(dialog).getByRole("button", { name: "Add expense", exact: true }),
    );
    await screen.findByText("Expense recorded. Balances have been refreshed.");
    expect(
      requests.find((r) => r.method === "post" && r.url.endsWith("/expenses"))
        .data,
    ).toEqual({ amount: 120, description: "Coffee", transactionType: 1 });
    expect(invalidation).toHaveBeenCalledWith({
      queryKey: ["event-expenses", eventId],
    });
    await waitFor(() =>
      expect(
        requests.filter((r) => r.url.endsWith("/all-event-expenses")).length,
      ).toBeGreaterThan(1),
    );
  });
  it("renders empty expenses and server errors distinctly", async () => {
    override = (config) =>
      config.url.endsWith("/all-event-expenses") ? { body: [] } : undefined;
    mount(`/events/${eventId}?tab=expenses`);
    await screen.findByText("No expenses yet.");
    cleanup();
    client.clear();
    override = (config) =>
      config.url.endsWith("/all-event-expenses")
        ? { status: 500, body: { message: "Expenses could not be loaded." } }
        : undefined;
    mount(`/events/${eventId}?tab=expenses`);
    await screen.findByText(
      "Expenses could not be loaded.",
      {},
      { timeout: 4000 },
    );
    expect(screen.queryByText("No expenses yet.")).toBeNull();
    expect(screen.getByRole("button", { name: "Try again" })).toBeTruthy();
  });
});

describe("shared event links", () => {
  it("copies the current event link and offers WhatsApp without username search", async () => {
    mount("/events/" + eventId);
    const user = userEvent.setup();
    const clipboard = vi
      .spyOn(navigator.clipboard, "writeText")
      .mockResolvedValue();
    await user.click(
      await screen.findByRole("button", { name: "Share event link" }),
    );
    const link = window.location.origin + "/accept-invite?eventId=" + eventId;
    expect(screen.getByLabelText("Event link").value).toBe(link);
    await user.click(screen.getByRole("button", { name: "Copy link" }));
    expect(clipboard).toHaveBeenCalledWith(link);
    await screen.findByText("Link copied. Paste it into WhatsApp or any chat.");
    expect(screen.getByRole("link", { name: "WhatsApp" }).href).toContain(
      encodeURIComponent(link),
    );
    expect(screen.queryByLabelText("Search by username")).toBeNull();
    expect(requests.some((r) => r.url.startsWith("/users/"))).toBe(false);
  });
  it("requires login and preserves the shared link destination", async () => {
    mount("/accept-invite?eventId=" + newEventId, false);
    await screen.findByRole("heading", { name: "Welcome back." });
    const user = userEvent.setup();
    await user.type(
      screen.getByLabelText("Email or username"),
      "jamie@example.test",
    );
    await user.type(screen.getByLabelText("Password"), "password123");
    await user.click(screen.getByRole("button", { name: "Sign in" }));
    await screen.findByText("Your request is waiting for admin approval.");
    expect(window.location.pathname + window.location.search).toBe(
      "/accept-invite?eventId=" + newEventId,
    );
  });
  it("requests once, waits for approval across reloads, then opens the approved event", async () => {
    mount("/accept-invite?eventId=" + newEventId);
    const user = userEvent.setup();

    await screen.findByText("Your request is waiting for admin approval.");
    expect(screen.queryByRole("link", { name: "Open event" })).toBeNull();
    expect(
      requests.filter(
        (r) => r.method === "post" && r.url.endsWith("/join-requests"),
      ),
    ).toHaveLength(1);
    cleanup();
    client.clear();
    mount("/accept-invite?eventId=" + newEventId);
    await screen.findByText("Your request is waiting for admin approval.");
    override = (config) =>
      config.url.endsWith("/join-request")
        ? { body: { eventName: "New test event", status: "active" } }
        : undefined;
    await user.click(
      screen.getByRole("button", { name: "Check approval status" }),
    );
    expect(
      (await screen.findByRole("link", { name: "Open event" })).getAttribute(
        "href",
      ),
    ).toBe("/events/" + newEventId);
  });
  it("does not offer sharing to non-admins", async () => {
    override = (config) =>
      config.url === "/events/" + eventId
        ? {
            body: {
              id: eventId,
              name: "Trip",
              isAdmin: false,
              members: [],
              createdAt: "2026-09-15",
            },
          }
        : undefined;
    mount("/events/" + eventId);
    await screen.findByRole("heading", { name: "Trip" });
    expect(
      screen.queryByRole("button", { name: "Share event link" }),
    ).toBeNull();
  });
  it("shows an invalid event response without creating a request", async () => {
    override = (config) =>
      config.url.endsWith("/join-request")
        ? { status: 400, body: { message: "Event not found." } }
        : undefined;
    mount("/accept-invite?eventId=" + newEventId);
    await screen.findByText("Event not found.");
    expect(
      screen.queryByRole("button", { name: "Request to join" }),
    ).toBeNull();
    expect(requests.some((r) => r.method === "post")).toBe(false);
  });
});

it("retries a failed automatic join without silently activating the user", async () => {
  let attempts = 0;
  override = (config) =>
    config.method === "post" &&
    config.url.endsWith("/join-requests") &&
    attempts++ === 0
      ? {
          status: 400,
          body: { message: "An existing Circlo account is required." },
        }
      : undefined;
  mount("/accept-invite?eventId=" + newEventId);
  await screen.findByText("An existing Circlo account is required.");
  expect(requests.filter((r) => r.method === "post")).toHaveLength(1);
  const user = userEvent.setup();
  await user.click(screen.getByRole("button", { name: "Retry join request" }));
  await screen.findByText("Your request is waiting for admin approval.");
  expect(requests.filter((r) => r.method === "post")).toHaveLength(2);
  expect(screen.queryByRole("link", { name: "Open event" })).toBeNull();
});
