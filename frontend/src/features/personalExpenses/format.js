export function money(amount, currency = "LKR") {
  if (amount == null) return "—";
  return new Intl.NumberFormat("en-LK", { style: "currency", currency, currencyDisplay: "code", maximumFractionDigits: 2 }).format(amount);
}
export function localDate(date = new Date()) {
  return `${String(date.getFullYear()).padStart(4, "0")}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`;
}
export function dateLabel(date) { return new Date(`${date.slice(0, 10)}T12:00:00`).toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" }); }
export const currentMonth = () => localDate().slice(0, 7);
export function monthParams(value) { const [year, month] = value.split("-").map(Number); return { year, month }; }
export function moveMonth(value, delta) { const date = new Date(`${value}-01T12:00:00`); date.setMonth(date.getMonth() + delta); return localDate(date).slice(0, 7); }
export const paymentMethods = ["Cash", "Debit card", "Credit card", "Bank transfer", "Digital wallet", "Other"];
