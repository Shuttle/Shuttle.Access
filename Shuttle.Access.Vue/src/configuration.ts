export interface Configuration {
  url: string;
  debugging: () => boolean;
  getApiUrl: (path: string) => string;
  getOAuthUrl: (name: string) => string
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

  getOAuthUrl(name) {
    let url = this.oauth[name];

    if (!url) {
        var providers = this.oauth[this.environment];

        if (!providers) {
            throw new Error(`Could not find oauth providers for environment '${this.environment}'.`);
        }

        url = providers[name];
    }

    if (!url) {
        throw new Error(`Could not find oauth provider url for name '${name}'.`);
    }

    return url.replace('{register}', register ? "-register" : "");
}
};

if (!import.meta.env.VITE_API_URL) {
  throw new Error("Configuration item 'VITE_API_URL' has not been set.");
}

if (Object.freeze) {
  Object.freeze(configuration);
}

export default configuration;
