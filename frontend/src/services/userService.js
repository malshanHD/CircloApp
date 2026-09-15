import api from "./api";
export const userService = {
  searchUsers: async (query, signal) =>
    query.trim()
      ? (await api.get("/users/search", { params: { q: query }, signal })).data
      : [],
};
