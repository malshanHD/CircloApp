import api from "./api";

// export const authService = async (path = '/', payload) =>{
//     return (await api.post(path, payload)).data;
// }

export const authService = async (path = "/", payload) => {
  try {
    return (await api.post(path, payload)).data;
  } catch (error) {
    const message =
      error.response?.data?.message ||
      "Something went wrong. Please try again.";

    throw new Error(message);
  }
};
