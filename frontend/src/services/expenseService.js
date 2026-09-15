import api from "./api";
export const expensesService = {
  getEventExpenses: async (eventId, signal) =>
    (await api.get(`/expenses/${eventId}/all-event-expenses`, { signal })).data,
  get: async (path = "/", signal) => (await api.get(path, { signal })).data,
  getSummary: async (eventId, signal) =>
    (await api.get(`/expenses/${eventId}`, { signal })).data,
  add: async (eventId, data) =>
    (await api.post(`/expenses/${eventId}/expenses`, data)).data,
};
