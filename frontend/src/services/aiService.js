import api from "./api";
export const askEvent = async (eventId, question, signal) => {
  const response = await api.post(
    "/Ask/ask",
    { eventId, question },
    { signal },
  );
  if (typeof response.data?.answer !== "string" || !response.data.answer.trim())
    throw new Error("Circlo AI returned an empty answer. Please try again.");
  return response.data;
};
