import api from "./api"

export const eventService = {
    getMyEvents: async () => {
        const response = await api.get("/events");
        return response.data;
    },

    createEvent: async (eventData) => {
        const response = await api.post("/events", eventData);
        return response.data;
    },

    addMemberToEvent: async (eventId, userData) => {
        const response = await api.post(`/events/${eventId}/members`, userData)
        return response.data;
    }
}