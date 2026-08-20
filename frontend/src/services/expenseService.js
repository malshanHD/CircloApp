import api from "./api"

export const expensesService = {
    get: async (path = "/") => {
        const response = await api.get(path);
        return response.data;
    }
}