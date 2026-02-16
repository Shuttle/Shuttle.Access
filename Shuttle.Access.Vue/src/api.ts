import axios from "axios";
import { useAlertStore } from "@/stores/alert";
import { useSessionStore } from "@/stores/session";
import configuration from "./configuration";
import router from "./router";

const api = axios.create({ baseURL: configuration.getUrl() });

api.interceptors.request.use(function (config) {
  const sessionStore = useSessionStore();

  config.headers["Authorization"] =
    `Shuttle.Access token=${sessionStore.token}`;
  config.headers["Shuttle-Access-Tenant-Id"] = `${sessionStore.tenantId}`;

  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      router.push("sign-in");

      return error;
    }

    const alertStore = useAlertStore();

    alertStore.add({
      message:
        error.response?.data ||
        error.response?.statusText ||
        "(unknown communication/network error)",
      type: "error",
      name: "api-error",
    });

    return Promise.reject(error);
  },
);

export default api;
