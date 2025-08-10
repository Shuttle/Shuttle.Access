import { defineStore } from "pinia";
import axios from "axios";
import configuration from "@/configuration";
import { i18n } from "@/i18n";
import type {
  SessionStoreState,
  Credentials,
  OAuthData,
  SessionResponse,
} from "@/access";

export const useSessionStore = defineStore("session", {
  state: (): SessionStoreState => {
    return {
      authenticated: false,
      initialized: false,
      identityName: "",
      token: "",
      permissions: [],
    };
  },
  getters: {
    status(): string {
      return !this.token ? "not-signed-in" : "signed-in";
    },
  },
  actions: {
    async initialize() {
      if (this.initialized) {
        return;
      }

      const identityName = localStorage.getItem("shuttle-access.identityName");
      const token = localStorage.getItem("shuttle-access.token");

      if (!!identityName && !!token) {
        try {
          return await this.signIn({
            identityName: identityName,
            token: token,
          });
        } finally {
          this.initialized = true;
        }
      }

      return Promise.resolve();
    },
    addPermission(permission: string) {
      if (this.hasPermission(permission)) {
        return;
      }

      this.permissions.push(permission);
    },
    register(session: SessionResponse) {
      if (
        !session ||
        !session.identityId ||
        !session.identityName ||
        !session.token ||
        !session.permissions
      ) {
        throw Error(i18n.global.t("messages.invalid-session"));
      }

      localStorage.setItem("shuttle-access.identityName", session.identityName);
      localStorage.setItem("shuttle-access.token", session.token);

      this.identityName = session.identityName;
      this.token = session.token;

      this.removePermissions();

      session.permissions.forEach((item: string) => {
        this.addPermission(item);
      });

      this.authenticated = true;
    },
    async signIn(credentials: Credentials): Promise<SessionResponse> {
      if (
        !credentials ||
        !credentials.identityName ||
        !(!!credentials.password || !!credentials.token)
      ) {
        throw new Error(i18n.global.t("messages.missing-credentials"));
      }

      const response = await axios.post<SessionResponse>(
        configuration.getApiUrl("v1/sessions"),
        {
          identityName: credentials.identityName,
          password: credentials.password,
          token: credentials.token,
          applicationName: credentials.applicationName,
        },
      );

      if (!response) {
        throw new Error("Argument 'response' may not be undefined.");
      }

      if (!response.data) {
        throw new Error("Argument 'response.data' may not be undefined.");
      }

      const sessionResponse = response.data;

      switch (sessionResponse.result) {
        case "Registered": {
          if (sessionResponse.sessionTokenExchangeUrl) {
            window.location.replace(sessionResponse.sessionTokenExchangeUrl);
            break;
          }

          this.register(sessionResponse);

          break;
        }
        case "UnknownIdentity": {
          break;
        }
        default: {
          throw new Error(i18n.global.t("exceptions.invalid-credentials"));
        }
      }

      return response.data;
    },
    async oauth(oauthData: OAuthData): Promise<SessionResponse> {
      if (!oauthData || !oauthData.state || !oauthData.code) {
        throw new Error(i18n.global.t("messages.oauth-missing-data"));
      }

      const response = await axios.get<SessionResponse>(
        configuration.getApiUrl(
          `v1/oauth/session/${oauthData.state}/${oauthData.code}`,
        ),
      );

      if (!response) {
        throw new Error("Argument 'response' may not be undefined.");
      }

      if (!response.data) {
        throw new Error("Argument 'response.data' may not be undefined.");
      }

      const sessionResponse = response.data;

      if (sessionResponse.result == "Registered") {
        this.register(sessionResponse);
      }

      this.initialized = true;

      return response.data;
    },
    signOut() {
      this.identityName = undefined;
      this.token = undefined;

      localStorage.removeItem("shuttle-access.identityName");
      localStorage.removeItem("shuttle-access.token");

      this.removePermissions();

      this.authenticated = false;
    },
    removePermissions() {
      this.permissions = [];
    },
    hasSession() {
      return !!this.token;
    },
    hasPermission(permission: string) {
      let result = false;
      const permissionRequired = permission.toLowerCase();

      this.permissions.forEach(function (item) {
        if (result) {
          return;
        }

        if (item.toLowerCase() === permissionRequired) {
          result = true;
          return;
        }

        if (item.indexOf("*") !== -1) {
          const escaped = item
            .replace(/[.*+?^${}()|[\]\\]/g, "\\$&")
            .replace(/\\\*/g, ".*");

          const regex = new RegExp(`^${escaped}$`, "i");
          if (regex.test(permissionRequired)) {
            result = true;
            return;
          }
        }
      });

      return result;
    },
  },
});
