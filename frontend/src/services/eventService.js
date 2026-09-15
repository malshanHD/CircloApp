import api from "./api";
export const eventService = {
  getJoinStatus: async (eventId, signal) =>
    (await api.get(`/events/${eventId}/join-request`, { signal })).data,
  approveJoin: async (eventId, userId) =>
    (await api.post(`/events/${eventId}/join-requests/${userId}/approve`)).data,
  getJoinRequests: async (signal) =>
    (await api.get("/events/join-requests", { signal })).data,
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

  requestJoin: async (eventId) =>
    (await api.post(`/events/${eventId}/join-requests`)).data,
};
