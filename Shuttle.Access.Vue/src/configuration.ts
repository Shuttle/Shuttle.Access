export interface Configuration {
  url: string;
  debugging: () => boolean;
  getApiUrl: (path: string) => string;
}

const configuration: Configuration = {
  url: `${import.meta.env.VITE_API_URL}${
    import.meta.env.VITE_API_URL.endsWith("/") ? "" : "/"
  }`,

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
