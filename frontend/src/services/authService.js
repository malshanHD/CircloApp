import api from "./api";
export const authService = async (path = "/", payload) =>
  (await api.post(path, payload)).data;
