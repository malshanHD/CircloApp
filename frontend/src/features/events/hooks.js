import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { eventService } from "../../services/eventService";
import { expensesService } from "../../services/expenseService";
export const isEventId = (value) =>
  /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(
    value || "",
  ) && value !== "00000000-0000-0000-0000-000000000000";
export function useEvents(page = 1, pageSize = 9) {
  return useQuery({
    queryKey: ["events", page, pageSize],
    queryFn: ({ signal }) =>
      eventService.getMyEvents({ page, pageSize, signal }),
  });
}
export function useEvent(eventId) {
  return useQuery({
    queryKey: ["event", eventId],
    queryFn: ({ signal }) => eventService.getDetails(eventId, signal),
    enabled: isEventId(eventId),
    retry: false,
  });
}
export function useSummary(eventId, enabled = true) {
  return useQuery({
    queryKey: ["event-summary", eventId],
    queryFn: ({ signal }) => expensesService.getSummary(eventId, signal),
    enabled: enabled && isEventId(eventId),
  });
}
export function useCreateEvent(onSuccess) {
  const client = useQueryClient();
  return useMutation({
    mutationFn: eventService.createEvent,
    onSuccess: (result) => {
      client.invalidateQueries({ queryKey: ["events"] });
      onSuccess(result);
    },
  });
}
export function useInvite(eventId, onSuccess) {
  const client = useQueryClient();
  return useMutation({
    mutationFn: (data) => eventService.addMemberToEvent(eventId, data),
    onSuccess: (result) => {
      client.invalidateQueries({ queryKey: ["event", eventId] });
      client.invalidateQueries({ queryKey: ["events"] });
      onSuccess(result);
    },
  });
}
export function useAcceptInvite(eventId, onSuccess) {
  const client = useQueryClient();
  return useMutation({
    mutationKey: ["accept-invite", eventId],
    mutationFn: () => eventService.acceptInvitation(eventId),
    onSuccess: async () => {
      await Promise.all([
        client.invalidateQueries({ queryKey: ["event-invitations"] }),
        client.invalidateQueries({ queryKey: ["event", eventId] }),
        client.invalidateQueries({ queryKey: ["events"] }),
        client.invalidateQueries({ queryKey: ["event-summary", eventId] }),
        client.invalidateQueries({ queryKey: ["event-expenses", eventId] }),
      ]);
      onSuccess();
    },
  });
}
export function useAddExpense(eventId, onSuccess) {
  const client = useQueryClient();
  return useMutation({
    mutationFn: (data) => expensesService.add(eventId, data),
    onSuccess: () => {
      for (const queryKey of [
        ["event-summary", eventId],
        ["event-expenses", eventId],
        ["my-expenses"],
        ["my-monthly-expenses"],
      ])
        client.invalidateQueries({ queryKey });
      onSuccess();
    },
  });
}
export function useExpenseHistory(year) {
  const events = useQuery({
    queryKey: ["my-expenses"],
    queryFn: ({ signal }) => expensesService.get("/expenses", signal),
  });
  const months = useQuery({
    queryKey: ["my-monthly-expenses", year],
    queryFn: ({ signal }) => expensesService.get(`/expenses/${year}`, signal),
  });
  return { events, months };
}

export function useInvitations() {
  return useQuery({
    queryKey: ["event-invitations"],
    queryFn: ({ signal }) => eventService.getInvitations(signal),
    staleTime: 15000,
    refetchInterval: 30000,
    refetchIntervalInBackground: false,
    retry: 1,
  });
}
export function useEventExpenses(eventId) {
  return useQuery({
    queryKey: ["event-expenses", eventId],
    queryFn: ({ signal }) => expensesService.getEventExpenses(eventId, signal),
    enabled: isEventId(eventId),
    retry: 1,
  });
}
