import { useMutation } from "@tanstack/react-query";
import { askEvent } from "../../services/aiService";
export function useAsk(eventId) {
  return useMutation({
    mutationKey: ["ask", eventId],
    mutationFn: ({ question, signal }) => askEvent(eventId, question, signal),
    retry: false,
  });
}
