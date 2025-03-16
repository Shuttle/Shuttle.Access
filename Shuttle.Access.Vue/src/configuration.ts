import type { Configuration, Env } from "./access";
import axios from "axios";

const env = async (): Promise<Env> => {
  if (import.meta.env.MODE === "production") {
    const response = await axios.get<Env>("/env");
    return response.data;
  } else {
    return {
      VITE_API_URL: import.meta.env.VITE_API_URL,
      VITE_API_ALLOW_PASSWORD_AUTHENTICATION: import.meta.env
        .VITE_API_ALLOW_PASSWORD_AUTHENTICATION,
    };
  }
};

const values = await env();

const configuration: Configuration = {
  url: `${values.VITE_API_URL}${values.VITE_API_URL.endsWith("/") ? "" : "/"}`,
  allowPasswordAuthentication:
    values.VITE_API_ALLOW_PASSWORD_AUTHENTICATION === "true",

  debugging() {
    return import.meta.env.DEV;
  },

  getApiUrl(path: string) {
    return this.url + path;
  },
};

if (!import.meta.env.VITE_API_URL) {
  throw new Error("Configuration item 'VITE_API_URL' has not been set.");
}

if (Object.freeze) {
  Object.freeze(configuration);
}

export default configuration;
