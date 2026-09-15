import axios from "axios";
import { auth } from "../utils/auth";
const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: { "Content-Type": "application/json" },
  timeout: 60000,
});
api.interceptors.request.use((config) => {
  if (!import.meta.env.VITE_API_BASE_URL)
    throw new Error(
      "Circlo's API URL is not configured. Set VITE_API_BASE_URL.",
    );
  const token = auth.getToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});
api.interceptors.response.use(
  (response) => {
    if (response.data?.success === false) {
      const error = new Error(response.data.message);
      error.response = response;
      throw error;
    }
    return response;
  },
  (error) => {
    // No refresh route exists. Never retry a rejected token or replay mutations.
    if (
      error.response?.status === 401 &&
      error.config?.headers?.Authorization === `Bearer ${auth.getToken()}`
    )
      auth.logout();
    return Promise.reject(error);
  },
);
export default api;
