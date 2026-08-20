import api from "./api"

export const eventService = {
    getMyEvents: async () => {
        const response = await api.get("/events");
        return response.data;
    }
}