import api from "./api";
export const authService = async (path = "/", payload, config) =>
  (await api.post(path, payload, config)).data;
