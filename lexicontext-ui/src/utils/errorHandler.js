import i18n from "../i18n";

export const extractErrorMessage = (error) => {
  if (error.response && error.response.data) {
    const data = error.response.data;
    const serverMessage = data.Message || data.message;

    if (serverMessage === "AI_UNAVAILABLE_AFTER_FALLBACK") {
      return i18n.t("errors.aiServiceOverloaded");
    }

    if (data.errors && typeof data.errors === "object") {
      const firstKey = Object.keys(data.errors)[0];
      const firstError = data.errors[firstKey];
      return Array.isArray(firstError) ? firstError[0] : firstError;
    }

    if (serverMessage) return serverMessage;
    if (data.detail) return data.detail;
  }

  if (error.code === "ECONNABORTED") {
    return i18n.t("errors.timeout");
  }

  if (error.request && !error.response) {
    return i18n.t("errors.noServerResponse");
  }

  return error.message || i18n.t("errors.unknown");
};