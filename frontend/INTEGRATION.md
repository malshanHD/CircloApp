# Circlo frontend integration

> Membership update (2026-09-15): the invitation routes and behavior described below are superseded by [shared links and admin approval](JOIN-REQUESTS.md). Other integration details remain unchanged.

Implemented against the repository's controllers, DTOs, validators, handlers, JWT configuration, and existing React services on 2026-09-14.

## Run locally

From the repository root:

```powershell
dotnet run --project src/CircloApp.API --launch-profile https
cd frontend
npm ci
npm run dev
```

The frontend requires Node compatible with the installed Vite 8 toolchain. The backend targets .NET 10.

The only frontend environment key is:

```dotenv
VITE_API_BASE_URL=https://localhost:7219/api
```

Use the backend's actual origin and include /api. The existing .env was preserved. Use http://localhost:5173 for the browser because that origin is in the current API CORS policy. Production hosting must serve index.html for client-side routes; the existing staticwebapp.config.json is preserved. Never add server credentials or Azure keys to VITE variables.

## What changed

- Kept JavaScript/JSX, React Router, TanStack Query, Axios, React Hook Form, Tailwind, React Icons, and the existing MUI chart integrations.
- Added Framer Motion for entrance and chat transitions; CSS and MotionConfig honor reduced motion.
- Added a violet/neutral design system, desktop sidebar, keyboard-accessible native mobile dialog, focus restoration, and responsive cards.
- Completed registration, login, OTP verification, protected destinations, logout, dashboard, events, create event, event details, member invitation, invitation acceptance, and event AI.
- Preserved the existing accessToken storage convention. A session provider reacts to login, logout, expiry, and cross-tab storage changes and clears Query caches on session changes.
- Added centralized errors that preserve backend message and validation details.
- Moved event/server state into feature hooks and retained service modules.
- Dashboard uses real paginated events and the existing expense/monthly endpoints. Removed sample statistics and inconsistent currency labels.
- Event details show API-provided totals and balances, an itemized expense/settlement list, active members, expense/settlement entry, and AI. No frontend financial calculations.
- AI uses POST /api/Ask/ask only. It prevents duplicate sends, supports Enter/Shift+Enter, cancels requests on navigation, clears conversations between events, supports retry, and renders plain text.
- Added a small backend guard to AskAiQueryHandler: an active, nondeleted membership is required before intent, search, or chat calls. The user's existing AskController.cs change was not edited.
- Enabled linting for JSX as well as TypeScript config files. Added isolated integration tests.
- Loaded charts and large pages in separate chunks.

## Endpoint inventory

ASP.NET's default JSON casing is camelCase. Auth endpoints return the envelope { success, message, data, errors }; other endpoints below return their bodies directly.

| Method | Exact route                                  | Request                                                           | Response                                                                         | Authorization / UI                                |
| ------ | -------------------------------------------- | ----------------------------------------------------------------- | -------------------------------------------------------------------------------- | ------------------------------------------------- |
| POST   | /api/Auth/register                           | { firstName, lastName, email, contactNumber, username, password } | Envelope data: { email, message, success, requiresOtpVerification }              | Public; registration                              |
| POST   | /api/Auth/login                              | { usernameOrEmail, password }                                     | Envelope data: { userId, username, email, accessToken, refreshToken, expiresAt } | Public; login                                     |
| POST   | /api/Auth/verify-email                       | { email, otp }                                                    | Envelope data: { success, message }                                              | Public; OTP                                       |
| GET    | /api/Events                                  | Query: Page (default 1), PageSize (default 10; 1–100)             | { items: EventSummary[], page, pageSize, totalCount, totalPages }                | JWT; dashboard/events                             |
| POST   | /api/Events                                  | { name, description }                                             | { eventId, message }                                                             | JWT; create event                                 |
| GET    | /api/Events/{eventId:guid}                   | None                                                              | { id, name, description, createdAt, members: Member[] }                          | JWT plus active membership in repository; details |
| POST   | /api/Events/{eventId:guid}/members           | { username, role }                                                | GUID string                                                                      | JWT plus event creator check; invite              |
| POST   | /api/Events/{eventId:guid}/accept-invitation | None                                                              | Message string                                                                   | JWT plus invitation check; accept                 |
| POST   | /api/Expenses/{eventId:guid}/expenses        | { amount: decimal, description, transactionType: 1 or 2 }         | ExpenseSummary                                                                   | JWT plus membership check; expense or settlement  |
| GET    | /api/Expenses/{eventId:guid}                 | None                                                              | ExpenseSummary                                                                   | JWT plus membership check; overview               |
| GET    | /api/Expenses                                | None                                                              | [{ totalExpenses, eventName, eventId }]                                          | JWT; dashboard chart                              |
| GET    | /api/Expenses/{year:int}                     | None                                                              | [{ totalAmount, month }]                                                         | JWT; monthly chart                                |
| GET    | /api/Users/{search}                          | Query q; frontend uses /api/Users/search?q=...                    | [{ username }]                                                                   | No authorization attribute; member search         |
| POST   | /api/Ask/ask                                 | { eventId: GUID, question }                                       | { answer: string }                                                               | JWT and new active-membership check; AI           |
| POST   | /api/Ask/embedding-test                      | { eventId, question } (eventId unused)                            | { dimensions, firstValues: number[] }                                            | JWT; diagnostic, no UI                            |
| GET    | /                                            | None                                                              | { status, app }                                                                  | Public; health                                    |

EventSummary: { id, name, description, createdAt, memberCount, isAdmin }.

Member: { userId, username, fullName, role }.

ExpenseSummary: { eventId, totalCost, equalSharePerPerson, userBalances: [{ userId, userName, totalPaid, totalSettled, totalSettledReceived, balance, status }] }.

TransactionType is numeric: Expense = 1; Settlement = 2. Member roles in the current backend use both Admin/admin and member casing. The invite handler assigns member itself regardless of the request's role.

Registration rules: names and username required, maximum 50 characters; email required and valid; password at least 8 characters. Contact number has no explicit validator. Event name is required, maximum 100; description is required by the command validator, maximum 500 despite being nullable in the DTO.

### Invitation and expense endpoint additions

| Method | Exact route                                     | Request | Response                                                                   | Authorization / UI                           |
| ------ | ----------------------------------------------- | ------- | -------------------------------------------------------------------------- | -------------------------------------------- |
| GET    | /api/Events/event-invite-notifications          | None    | { invitationsCount, inviteDetails: [{ eventId, inviterName, eventName }] } | JWT; notification bell and invitation dialog |
| GET    | /api/Expenses/{eventId:guid}/all-event-expenses | None    | [{ paidUser, description, amount, dateAndTime, type }]                     | JWT and membership check; Expenses tab       |

- Notifications use the server's invitationsCount, show "{inviterName} invited you to {eventName}", and refresh every 30 seconds while visible, on refocus when stale, and manually.
- Accept calls POST /api/Events/{eventId}/accept-invitation. No client-only enrollment is simulated: success invalidates invitations, active events, details, summary, and expenses, then opens the joined event.
- Acceptance errors stay next to the invitation and do not decrement its count. Duplicate acceptance clicks are disabled while submitting.
- The expense tab displays all returned records, including settlements (type 2) and expenses (type 1), with search and type filtering. Adding an expense or settlement refreshes the list and summary.
- A single compatibility fix makes GetEventAllExpensesQueryHandler return [] when the event has no expenses instead of throwing a generic 500. The user's new endpoint routes and DTOs are unchanged.
- Existing invitation email sending remains in the backend; notification display does not depend on email delivery. No email was sent during fixture tests.

### Deprecated AIController routes

These were inspected but are not called by the redesigned frontend. AIController has no authorization attribute and the app has no fallback authorization policy.

| Method | Route                                    | Request                               | Response                                                                  |
| ------ | ---------------------------------------- | ------------------------------------- | ------------------------------------------------------------------------- |
| GET    | /api/AI/{eventId:guid}                   | None                                  | { expenses: [{ expenseId, category }] }                                   |
| GET    | /api/AI/events/{eventId:guid}/categories | None                                  | { totalExpense, categories: [{ category, amount, percentage }], summary } |
| GET    | /api/AI/test                             | Query text                            | { text, dimensions, first10Values }                                       |
| POST   | /api/AI/events/{eventId:guid}/ask        | { question, eventId } (route ID used) | { answer }                                                                |
| GET    | /api/AI/embedding/similarity-test        | None                                  | { uberVsTaxi, uberVsFood }                                                |
| POST   | /api/AI/search/create-index              | None                                  | { message }                                                               |
| POST   | /api/AI/search/upload-test-expense       | None                                  | { message, expenseId, eventId }                                           |
| DELETE | /api/AI/search/delete-index              | None                                  | { message }                                                               |
| POST   | /api/AI/search/upload-sample-expenses    | None                                  | { message, eventId }                                                      |
| GET    | /api/AI/search/vector                    | Query eventId, query                  | [{ id, description, amount, score }]                                      |
| POST   | /api/AI/rag/{eventId:guid}               | { question, eventId } (route ID used) | { answer }                                                                |
| POST   | /api/AI/ask/{eventId:guid}               | { question, eventId } (route ID used) | { answer }                                                                |

## Authentication behavior and limitations

- JWT validates issuer, audience, signature, and lifetime. Browser-decoded identity is display-only; it grants no API permission.
- JWT expiry is configured by JwtSettings.ExpiryMinutes. LoginResponse.ExpiresAt is separately hardcoded to 20 minutes, so the frontend uses the actual token's exp claim.
- Login issues a refresh token and persists it with a seven-day expiry, but there is no refresh route. The frontend deliberately makes no refresh calls and does not persist an unusable refresh token.
- Logout clears local tokens and cached server data. No server revocation/logout endpoint exists.
- A protected 401 clears the matching current session. Other HTTP errors remain visible without logging the user out.
- OTP generation uses six digits and records a five-minute expected expiry. The UI preserves the expected countdown across reloads. No resend endpoint exists.
- Backend OTP expiry handling is inconsistent: pending registrations are initially cached for ten minutes, and VerifyOtpCommandHandler does not explicitly compare OtpExpiresAt before accepting a valid code. The UI countdown is informational; backend expiry enforcement needs a separate fix.

## Remaining backend gaps

1. The current Ask calculation branch returns selected expense IDs joined with newlines instead of invoking the financial calculation service. The UI shows the returned answer unchanged. This needs completing before financial AI answers can be accepted end to end.
2. Ask returns only answer. There are no citations or source records to display, and no server conversation-history endpoint. Conversations are held in component state for the current event.
3. The new itemized expense endpoint is integrated. It does not return transaction IDs; the ledger is read-only and does not expose unsupported edit/delete actions.
4. No refresh, OTP resend, server token revocation, event status, event edit/delete, or current-user profile endpoint was found.
5. Invitation email links use a hardcoded localhost base and the send call is not awaited. The UI supports /accept-invite?eventId=..., but delivery and correct hosted links require a backend change.
6. Deprecated AI/search administration endpoints remain unauthenticated. They are not used by this frontend and should be secured or removed in a backend hardening change.
7. Event lists now include only active memberships. Pending invitations come from the new notification endpoint. Accepting an invitation refreshes the notification count and active event list from the server.
8. Expense/settlement validation and allocation remain authoritative on the server. The frontend validates a positive numeric amount but does not recalculate or correct server balances.

## Verification

Commands:

```powershell
cd frontend
npm test
npm run lint
npm run build
cd ..
dotnet test tests/CircloApp.Application.Tests/CircloApp.Application.Tests.csproj --no-restore
```

Results:

- Frontend integration suite: 17 tests passed.
- Lint: passed, including JS/JSX.
- Production build: passed; no oversized-chunk warning after page/chart splitting.
- Backend application suite: 12 tests passed (7 existing, 4 AI authorization cases, and 1 empty-expense response test).
- Initial full solution test command completed; API and domain test projects contain no runnable tests.
- Existing backend warnings remain (nullable references and unawaited invitation email).

Browser checks at desktop 1440×1000 and mobile 390×844:

- Protected-route redirect, required-field focus, password fields, registration validation.
- Fixture-backed registration duplicate error and success, OTP invalid/success and six-digit paste.
- Fixture-backed login failure/success, exact backend messages, restored event destination, logout.
- Dashboard/event rendering, mobile drawer creation flow, creation success navigation.
- AI suggested question success, Enter submission, failure display, and conversation isolation between events.
- Native drawer Escape dismissal restores focus to its opener.
- Fixture-backed invitation count, inviter/event wording, acceptance navigation, and updated count; event expense list and transaction filters.
- No horizontal document overflow on checked mobile event layout.
- Browser error/warning log inspection was clean for the inspected UI; failures deliberately returned by fixtures are test scenarios.
- Automated flows run with prefers-reduced-motion matched; CSS also disables transitions/animations under that media query.

Live integration limitation: the configured https://localhost:7219 API initially refused connections. Starting the existing HTTPS profile failed during Key Vault setup because DefaultAzureCredential could not authenticate. No live email, database write, refresh, or Azure AI success is claimed.

### Isolated browser fixtures

Only use these for development verification; they are outside src and are never imported by the application:

```powershell
# Terminal 1, frontend/
npm run test:fixtures

# Terminal 2, frontend/
$env:VITE_API_BASE_URL = "http://127.0.0.1:4179/api"
npm run dev -- --host 127.0.0.1 --port 5174 --strictPort
```

Open http://127.0.0.1:5174. All data is synthetic. Login with jamie@example.test and any password except wrong. OTP is 123456. duplicate@example.test produces the registration error. An AI question containing fail produces the AI error. Fixture creation does not persist data and is not evidence of backend writes.

## Recommended next work

Complete the backend AI calculation branch and add end-to-end tests against an authenticated development API. Then add refresh and OTP resend endpoints, fix invitation links, and secure the deprecated AI routes.
