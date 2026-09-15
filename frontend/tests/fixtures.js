// Synthetic API fixtures for tests only. Never imported by production code.
export const eventId = "b1670404-1b08-4e5d-809a-d654a77955a4";
export const newEventId = "79c5060b-7123-4906-a1db-cebe42beb27a";
export const userId = "7f959e6e-75b3-4f45-88ae-c2ad33b0d45b";
export const token = () =>
  `test.${btoa(JSON.stringify({ exp: Math.floor(Date.now() / 1000) + 1200, unique_name: "Jamie", email: "jamie@example.test" }))}.test`;
export const events = [
  {
    id: eventId,
    name: "Weekend in the hills",
    description: "Fresh air, good company, and a long weekend away.",
    createdAt: "2026-09-10T12:00:00Z",
    memberCount: 4,
    isAdmin: true,
  },
  {
    id: "a97321be-e55d-4087-ae7a-264cd4f70174",
    name: "Sunday supper club",
    description: "Something homemade. Everyone welcome.",
    createdAt: "2026-09-08T12:00:00Z",
    memberCount: 6,
    isAdmin: false,
  },
  {
    id: "c57321be-e55d-4087-ae7a-264cd4f70174",
    name: "The coast is calling",
    description: "A few days by the sea with our favorite people.",
    createdAt: "2026-09-07T12:00:00Z",
    memberCount: 3,
    isAdmin: true,
  },
];
export const summary = {
  eventId,
  totalCost: 2400,
  equalSharePerPerson: 600,
  userBalances: [
    {
      userId,
      userName: "Jamie",
      totalPaid: 1200,
      totalSettled: 0,
      totalSettledReceived: 0,
      balance: 600,
      status: "Owed 600",
    },
  ],
};
const initialInvitations = [
  {
    eventId: newEventId,
    fullName: "Malshan Perera",
    userId,
    username: "malshan",
    eventName: "New test event",
  },
  {
    eventId: "d57321be-e55d-4087-ae7a-264cd4f70174",
    fullName: "Alex Silva",
    userId,
    username: "alex",
    eventName: "Beach day",
  },
];
const acceptedInvitations = new Set();
const requestedEvents = new Set();
export function resetFixtureState() {
  acceptedInvitations.clear();
  requestedEvents.clear();
}
export const expenseRows = [
  {
    paidUser: "Jamie Taylor",
    description: "Train tickets",
    amount: 800,
    dateAndTime: "2026-09-10T09:00:00Z",
    type: 1,
  },
  {
    paidUser: "Alex Silva",
    description: "Group lunch",
    amount: 600,
    dateAndTime: "2026-09-11T12:00:00Z",
    type: 1,
  },
  {
    paidUser: "Sam Perera",
    description: "Settlement payment to Jamie",
    amount: 300,
    dateAndTime: "2026-09-12T15:00:00Z",
    type: 2,
  },
];
export function fixture(method, path, data = {}) {
  const url = new URL(path, "http://fixture");
  const route = url.pathname.toLowerCase().replace(/^\/api/, "");
  const envelope = (value) => ({
    success: true,
    message: "Success",
    data: value,
    errors: null,
  });
  const failure = (message) => ({
    status: 400,
    body: { success: false, message, data: null, errors: null },
  });
  if (route === "/auth/login")
    return data.password === "wrong"
      ? failure("Invalid username/email or password.")
      : {
          body: envelope({
            userId,
            username: "Jamie",
            email: "jamie@example.test",
            accessToken: token(),
            refreshToken: "fixture-unused",
            expiresAt: new Date(Date.now() + 1200000).toISOString(),
          }),
        };
  if (route === "/auth/register")
    return data.email === "duplicate@example.test"
      ? failure("User with the specified email already exists.")
      : {
          body: envelope({
            email: data.email,
            success: true,
            message: "OTP has been sent to your email.",
            requiresOtpVerification: true,
          }),
        };
  if (route === "/auth/verify-email")
    return data.otp === "123456"
      ? {
          body: envelope({
            success: true,
            message: "Email Verified Successfully",
          }),
        }
      : failure("Invalid OTP");
  if (route === "/events" && method === "POST")
    return data.name === "Invalid"
      ? {
          status: 400,
          body: {
            message: "Validation failed.",
            errors: ["Name is not valid."],
          },
        }
      : {
          body: { eventId: newEventId, message: "Event Created Successfully" },
        };
  if (route === "/events/join-requests") {
    const inviteDetails = initialInvitations.filter(
      (invite) => !acceptedInvitations.has(invite.eventId),
    );
    return { body: inviteDetails };
  }
  if (route.endsWith("/join-request"))
    return {
      body: {
        eventName: "New test event",
        status: acceptedInvitations.has(route.split("/")[2])
          ? "active"
          : requestedEvents.has(route.split("/")[2])
            ? "pending"
            : "not-requested",
      },
    };
  if (route.endsWith("/join-requests") && method === "POST") {
    requestedEvents.add(route.split("/")[2]);
    return { body: { eventName: "New test event", status: "pending" } };
  }
  if (route.endsWith("/all-event-expenses")) return { body: expenseRows };
  if (route.endsWith("/approve")) {
    acceptedInvitations.add(route.split("/")[2]);
    return { body: "Invitation accepted successfully." };
  }
  if (route === "/events")
    return {
      body: {
        items: [
          ...events,
          ...initialInvitations
            .filter((invite) => acceptedInvitations.has(invite.eventId))
            .map((invite) => ({
              ...events[0],
              id: invite.eventId,
              name: invite.eventName,
              isAdmin: false,
            })),
        ],
        page: 1,
        pageSize: 9,
        totalCount: 3 + acceptedInvitations.size,
        totalPages: 1,
      },
    };
  if (/^\/events\/[^/]+$/.test(route))
    return {
      body: {
        ...(events.find((e) => route.endsWith(e.id)) || {
          ...events[0],
          id: newEventId,
          name: "New test event",
        }),
        members: [
          {
            userId,
            username: "Jamie",
            fullName: "Jamie Taylor",
            role: "Admin",
          },
        ],
      },
    };
  if (route.endsWith("/members")) return { body: userId };
  if (route.endsWith("/accept-invitation"))
    return { body: "Invitation accepted successfully." };
  if (route === "/expenses")
    return {
      body: events.map((event, i) => ({
        eventId: event.id,
        eventName: event.name,
        totalExpenses: [2400, 900, 1800][i],
      })),
    };
  if (/^\/expenses\/\d+$/.test(route))
    return {
      body: [
        { month: "July", totalAmount: 1200 },
        { month: "August", totalAmount: 1500 },
        { month: "September", totalAmount: 2400 },
      ],
    };
  if (route.startsWith("/expenses/")) return { body: summary };
  if (route === "/ask/ask")
    return data.question.toLowerCase().includes("fail")
      ? {
          status: 500,
          body: {
            success: false,
            message: "The AI service is temporarily unavailable.",
            errors: null,
          },
        }
      : {
          body: {
            answer:
              "For this test event, recorded expenses total 2,400. Transport accounts for 800. This is an isolated test response.",
          },
        };
  if (route.startsWith("/users/"))
    return { body: [{ username: "alex" }, { username: "sam" }] };
  return { status: 404, body: { message: "No fixture for this route." } };
}
