import api from "./api";
const clean = params => Object.fromEntries(Object.entries(params || {}).filter(([, value]) => value !== "" && value != null));
export const personalExpenseService = {
  get: async (path, params, signal) => (await api.get(path, { params: clean(params), signal })).data,
  save: async (expense, id) => (await (id ? api.put(`/personal-expenses/${id}`, expense) : api.post("/personal-expenses", expense))).data,
  remove: id => api.delete(`/personal-expenses/${id}`),
  settings: data => api.put("/personal-expense-settings", data),
  budget: ({ year, month, limitAmount }) => api.put(`/personal-expense-budgets/${year}/${month}`, { limitAmount }),
  removeBudget: ({ year, month }) => api.delete(`/personal-expense-budgets/${year}/${month}`),
};
