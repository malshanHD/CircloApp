export function getApiError(error) {
  const body = error?.response?.data;
  const raw = body?.errors;
  const errors = Array.isArray(raw)
    ? raw
    : raw && typeof raw === "object"
      ? Object.values(raw).flat()
      : [];
  const fallbacks = {
    400: "Please check your information.",
    401: "Your session has expired. Please sign in again.",
    403: "You do not have access to this action.",
    404: "We could not find what you requested.",
    500: "Something went wrong on the server. Please try again.",
  };
  const message =
    body?.message ||
    body?.title ||
    fallbacks[error?.response?.status] ||
    (error?.response
      ? "The request could not be completed."
      : error?.isAxiosError
        ? "Unable to reach Circlo. Check your connection and try again."
        : error?.message || "Something went wrong. Please try again.");
  return { message, errors: errors.filter((item) => typeof item === "string") };
}
