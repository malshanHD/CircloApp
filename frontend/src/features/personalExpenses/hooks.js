import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { personalExpenseService as service } from "../../services/personalExpenseService";
export function usePersonalQuery(path, params = {}) {
  return useQuery({ queryKey: ["personal-expenses", path, params],
    queryFn: ({ signal }) => service.get(path, params, signal), retry: false });
}
export function usePersonalMutation(mutationFn, onSuccess) {
  const client = useQueryClient();
  return useMutation({ mutationFn, retry: false, onSuccess: async () => {
    await client.invalidateQueries({ queryKey: ["personal-expenses"] });
    onSuccess?.();
  } });
}
