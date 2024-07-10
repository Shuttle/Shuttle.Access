import axios from "axios";
import { useAlertStore } from "@/stores/alert";
import { useSessionStore } from "@/stores/session";
import configuration from "./configuration";
import router from "./router";

const api = axios.create({ baseURL: configuration.url });
const alertStore = useAlertStore();
const sessionStore = useSessionStore();

api.interceptors.request.use(function (config) {
  config.headers["Authorization"] = `Bearer ${sessionStore.token}`;

  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response.status == 401) {
      router.push("signin");

      return error;
    }

    alertStore.add({
      message:
        error.response.data ||
        error.response.statusText ||
        "(unknown communication/network error)",
      type: "error",
      name: "api-error",
    });

    return Promise.reject(error);
  }
);

export default api;
