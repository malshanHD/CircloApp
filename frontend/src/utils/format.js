export const formatAmount = (value) =>
  typeof value === "number"
    ? new Intl.NumberFormat(undefined, {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
      }).format(value)
    : "—";
export const formatDate = (value) =>
  value && !Number.isNaN(Date.parse(value))
    ? new Intl.DateTimeFormat(undefined, {
        month: "short",
        day: "numeric",
        year: "numeric",
      }).format(new Date(value))
    : "Date unavailable";
