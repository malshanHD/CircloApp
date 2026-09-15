# Shared event links and admin approval

This change replaces username/email invitations with requests from existing signed-in users. Other application flows retain their existing behavior.

## User flow

1. An event creator uses **Share event link** to copy a link, open a WhatsApp message draft, or use the device share sheet. Sending is controlled by the user.
2. The recipient signs in to an existing Circlo account. The login redirect preserves the event link.
3. After sign-in, opening the join page automatically submits a POST request and creates one EventMembers row with IsActive=false. Repeated visits preserve the existing status. Failed submissions show a retry button; unauthenticated link previews cannot create requests.
4. The creator sees pending requests in the notification bell, including applicant name, username, and event. **Approve request** sets IsActive=true and updates JoinedAt/UpdatedAt.
5. The recipient's join page refreshes approval status every 15 seconds and offers **Open event** after approval. Manual refresh is also available.

## API contracts (all require JWT)

- GET /api/Events/{eventId}/join-request returns {eventName,status}; status is not-requested, pending, or active. Does not create a membership or reveal event members/expenses.
- POST /api/Events/{eventId}/join-requests takes no body. Uses the authenticated user's ID, verifies that account and event exist, and returns {eventName,status}.
- GET /api/Events/join-requests returns [{eventId,userId,eventName,fullName,username}] for pending members in events created by the authenticated admin.
- POST /api/Events/{eventId}/join-requests/{userId}/approve takes no body. Only the event creator can approve an existing pending membership. Repeated approval succeeds without changing the activation timestamp.
- Event details additionally returns isAdmin for showing the share button. Server authorization remains authoritative.
- Old POST /members and POST /accept-invitation routes return HTTP 410. Legacy MediatR handlers also reject the old workflow; invitees cannot activate themselves.
- The old event-invite-notifications route is replaced by join-requests.

## Database and access

No schema migration is needed. The existing unique (EventId,UserId) index prevents duplicate rows. Concurrent SQL Server unique-key conflicts recover the existing request instead of activating or duplicating it.

Inactive members cannot access current event-detail, expense read/write, or Ask AI flows. Active-member queries exclude pending/deleted rows from participant counts and settlement allocations. The arithmetic itself is unchanged.

Existing inactive invitation records remain pending and can be approved by their event creator. No existing membership rows are deleted or automatically activated. Shared links use the event ID; possessing a link permits requesting access, never bypassing approval.

## Verification

Frontend regression tests cover sharing/copy, login return path, pending status across remounts, approval refresh, admin visibility, failed requests, duplicate approval submissions, notification count refresh, and existing expense/auth/event/AI UI flows. Backend tests cover account/event existence, pending creation, status preservation, creator-only approval, absent requests, retired self-acceptance, and inactive-member access.

The existing AI authorization test mock was updated for the repository's newer AnalyzeAsync API and added constructor dependency; the AI implementation was not changed.

Live database, Azure authentication, and external messaging are not exercised by the isolated tests. Run both updated frontend and backend together; restart an older running API before testing the new routes.
