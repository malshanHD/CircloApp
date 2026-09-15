import api from "./api";
export const eventService = {
  getInvitations: async (signal) =>
    (await api.get("/events/event-invite-notifications", { signal })).data,
  getMyEvents: async ({ page = 1, pageSize = 9, signal } = {}) =>
    (
      await api.get("/events", {
        params: { Page: page, PageSize: pageSize },
        signal,
      })
    ).data,
  getDetails: async (eventId, signal) =>
    (await api.get(`/events/${eventId}`, { signal })).data,
  createEvent: async (eventData) => (await api.post("/events", eventData)).data,
  addMemberToEvent: async (eventId, userData) =>
    (await api.post(`/events/${eventId}/members`, userData)).data,
  acceptInvitation: async (eventId) =>
    (await api.post(`/events/${eventId}/accept-invitation`)).data,
};
