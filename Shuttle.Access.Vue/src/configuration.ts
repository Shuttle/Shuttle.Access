import type { Configuration, Env, ServerConfiguration } from "./access";
import axios from "axios";

const env = async (): Promise<Env> => {
  if (import.meta.env.MODE === "production") {
    return (await axios.get<Env>("/env")).data;
  } else {
    return {
      VITE_API_URL: import.meta.env.VITE_API_URL,
    };
  }
};

const values = await env();

const serverConfiguration: ServerConfiguration = (
  await axios.get<ServerConfiguration>(
    `${values.VITE_API_URL}${values.VITE_API_URL.endsWith("/") ? "" : "/"}v1/server/configuration`,
  )
).data;

const getConfiguration = (): Configuration => {
  return {
    url: `${values.VITE_API_URL}${values.VITE_API_URL.endsWith("/") ? "" : "/"}`,
    allowPasswordAuthentication:
      serverConfiguration.allowPasswordAuthentication,

    debugging() {
      return import.meta.env.DEV;
    },

    getApiUrl(path: string) {
      return this.url + path;
    },
  };
};

const configuration = getConfiguration();

if (!import.meta.env.VITE_API_URL) {
  throw new Error("Configuration item 'VITE_API_URL' has not been set.");
}

if (Object.freeze) {
  Object.freeze(configuration);
}

export default configuration;
