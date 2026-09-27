import { act, cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, expect, it, vi } from "vitest";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import App from "../src/App";
import GoogleAuthProvider from "../src/components/GoogleAuthProvider";
import api from "../src/services/api";
import { fixture, token, eventId, resetFixtureState } from "./fixtures";
import { loginDestination } from "../src/utils/loginDestination";
let client, callback, requests, googleResponse;
beforeEach(() => {
  localStorage.clear(); sessionStorage.clear(); resetFixtureState(); requests=[];
  googleResponse = async () => ({status:200,data:{success:true,data:{accessToken:token(),refreshToken:"circlo-refresh",username:"Jamie"}}});
  window.google = { accounts: { id: {
    initialize: vi.fn(options => { callback=options.callback; }),
    renderButton: vi.fn(container => {
      const button=document.createElement("button"); button.textContent="Continue with Google";
      button.onclick=()=>callback({credential:"google-id-token"}); container.replaceChildren(button);
    }),
  }}};
  api.defaults.adapter=async config => {
    const data=typeof config.data === "string" ? JSON.parse(config.data) : config.data;
    requests.push({url:config.url,data});
    let response;
    if(config.url === "/auth/google") response=await googleResponse();
    else { const f=fixture(config.method.toUpperCase(),config.url,data); response={status:f.status||200,data:f.body}; }
    response={...response,config,headers:{}};
    if(response.status>=400) throw Object.assign(new Error("Request failed"), {response,config,isAxiosError:true});
    return response;
  };
});
afterEach(()=>{cleanup();client?.clear();delete window.google;vi.restoreAllMocks();});
function mount(path="/login",clientId="test.apps.googleusercontent.com") {
  window.history.replaceState({},"",path);
  client=new QueryClient({defaultOptions:{queries:{retry:false},mutations:{retry:false}}});
  render(<GoogleAuthProvider clientId={clientId}><QueryClientProvider client={client}><App /></QueryClientProvider></GoogleAuthProvider>);
}
async function loadGoogle() {
  await act(async()=> document.querySelector('script[src="https://accounts.google.com/gsi/client"]').dispatchEvent(new Event("load")));
  await screen.findByRole("button",{name:"Continue with Google"});
}
it("passes the credential through authService and restores the deep link",async()=>{
  mount(`/events/${eventId}?tab=ai`); await loadGoogle();
  await userEvent.setup().click(screen.getByRole("button",{name:"Continue with Google"}));
  await waitFor(()=>expect(window.location.pathname+window.location.search).toBe(`/events/${eventId}?tab=ai`));
  expect(requests.find(r=>r.url==="/auth/google").data).toEqual({idToken:"google-id-token"});
  expect(localStorage.getItem("accessToken")).toMatch(/^test\./);
  expect(localStorage.getItem("accessToken")).not.toBe("google-id-token");
});
it("locks duplicate Google callbacks and password submit without reinitializing the SDK",async()=>{
  let release; googleResponse=()=>new Promise(resolve=>{release=resolve;});
  mount(); await loadGoogle(); const user=userEvent.setup();
  await user.type(screen.getByLabelText("Email or username"),"Jamie");
  await user.type(screen.getByLabelText("Password"),"password123");
  act(()=>{callback({credential:"first"});callback({credential:"second"});});
  await waitFor(()=>expect(requests.filter(r=>r.url==="/auth/google")).toHaveLength(1));
  expect(screen.getByLabelText("Password").closest("fieldset").disabled).toBe(true);
  fireEvent.submit(screen.getByLabelText("Password").closest("form"));
  expect(requests.some(r=>r.url==="/auth/login")).toBe(false);
  await act(async()=>release({status:400,data:{message:"Invalid Google credential."}}));
  await screen.findByText("Invalid Google credential.");
  expect(window.google.accounts.id.initialize).toHaveBeenCalledTimes(1);
  expect(document.querySelectorAll('script[src="https://accounts.google.com/gsi/client"]')).toHaveLength(1);
});
it("shows friendly errors for Google failure and malformed API responses",async()=>{
  mount(); await loadGoogle(); act(()=>callback({}));
  await screen.findByText("Google sign-in was not completed. Please try again or use your password.");
  expect(requests.some(r=>r.url==="/auth/google")).toBe(false);
  googleResponse=async()=>({status:200,data:{data:{}}});
  await userEvent.setup().click(screen.getByRole("button",{name:"Continue with Google"}));
  await screen.findByText("Sign-in could not be completed. Please try again.");
  expect(localStorage.getItem("accessToken")).toBeNull();
});
it("shows script loading failure while retaining password login",async()=>{
  mount(); await act(async()=>document.querySelector('script[src="https://accounts.google.com/gsi/client"]').dispatchEvent(new Event("error")));
  await screen.findByText("Google sign-in could not load. Check your connection or sign in with your password.");
  expect(screen.getByRole("button",{name:"Sign in",exact:true}).disabled).toBe(false);
});
it("renders an accessible startup error when Client ID is missing",()=>{
  mount("/login",""); expect(screen.getByRole("alert").textContent).toContain("VITE_GOOGLE_CLIENT_ID is missing");
  expect(document.querySelector('script[src="https://accounts.google.com/gsi/client"]')).toBeNull();
});
it("rejects unsafe destinations and authentication loops",()=>{
  for(const path of ["https://evil.test","//evil.test","/\\evil.test","/login","/register",undefined]) expect(loginDestination(path)).toBe("/dashboard");
  expect(loginDestination("/accept-invite?eventId=abc")).toBe("/accept-invite?eventId=abc");
});

it("waits for the API to wake and resumes Google login without another click", async () => {
  const originalGet = api.get.bind(api);
  let probes = 0;
  vi.spyOn(api, "get").mockImplementation((url, config) => {
    if (url === "/health" && ++probes === 1) return Promise.reject({ response: { status: 503 } });
    return originalGet(url, config);
  });
  mount(); await loadGoogle();
  await userEvent.click(screen.getByRole("button", { name: "Continue with Google" }));
  await screen.findByText("Waking up Circlo… 😴");
  expect(requests.filter(r => r.url === "/auth/google")).toHaveLength(0);
  await waitFor(() => expect(requests.filter(r => r.url === "/auth/google")).toHaveLength(1), { timeout: 4500 });
  await waitFor(() => expect(localStorage.getItem("accessToken")).toMatch(/^test\./));
});
