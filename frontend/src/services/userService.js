import api from "./api"

export const userService = {
  searchUsers: async (query) => {
    if (!query || query.trim() === '') return [];
    const response = await api.get(`/users/search?q=${encodeURIComponent(query)}`)
    return response.data // Expected array of user objects: [{ id, firstName, lastName, username, email }]
  }
}