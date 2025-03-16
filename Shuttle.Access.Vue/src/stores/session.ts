import { defineStore } from "pinia";
import axios from "axios";
import configuration from "@/configuration";
import { i18n } from "@/i18n";
import type {
  Session,
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
    addPermission(type: string, permission: string) {
      if (this.hasPermission(permission)) {
        return;
      }

      this.permissions.push({ type: type, permission: permission });
    },
    register(session: Session) {
      if (
        !session ||
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

      this.removePermissions("identity");

      session.permissions.forEach((item: string) => {
        this.addPermission("identity", item);
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

      const data = response.data;

      switch (data.result) {
        case "Registered": {
          if (data.sessionTokenExchangeUrl) {
            window.location.replace(data.sessionTokenExchangeUrl);
            break;
          }

          this.register({
            identityName: credentials.identityName,
            token: data.token,
            permissions: data.permissions,
          });

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

      const data = response.data;

      if (data.result == "Registered") {
        this.register({
          identityName: data.identityName,
          token: data.token,
          permissions: data.permissions,
        });
      }

      this.initialized = true;

      return response.data;
    },
    signOut() {
      this.identityName = undefined;
      this.token = undefined;

      localStorage.removeItem("shuttle-access.identityName");
      localStorage.removeItem("shuttle-access.token");

      this.removePermissions("identity");

      this.authenticated = false;
    },
    removePermissions(type: string) {
      this.permissions = this.permissions.filter(function (item) {
        return item.type !== type;
      });
    },
    hasSession() {
      return !!this.token;
    },
    hasPermission(permission: string) {
      let result = false;
      const permissionCompare = permission.toLowerCase();

      this.permissions.forEach(function (item) {
        if (result) {
          return;
        }

        result =
          item.permission === "*" ||
          item.permission.toLowerCase() === permissionCompare;
      });

      return result;
    },
  },
});
