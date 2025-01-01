import { defineStore } from "pinia";
import axios, { type AxiosResponse } from "axios";
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
      const self = this;

      if (this.initialized) {
        return;
      }

      const identityName = localStorage.getItem("shuttle-access.identityName");
      const token = localStorage.getItem("shuttle-access.token");

      if (!!identityName && !!token) {
        return self
          .signIn({ identityName: identityName, token: token })
          .then(function (response: any) {
            return response;
          })
          .finally(() => {
            self.initialized = true;
          });
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
      const self = this;

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

      self.identityName = session.identityName;
      self.token = session.token;

      self.removePermissions("identity");

      session.permissions.forEach((item: string) => {
        self.addPermission("identity", item);
      });

      this.authenticated = true;
    },
    async signIn(
      credentials: Credentials
    ): Promise<AxiosResponse<SessionResponse>> {
      const self = this;

      return new Promise((resolve, reject) => {
        if (
          !credentials ||
          !credentials.identityName ||
          !(!!credentials.password || !!credentials.token)
        ) {
          reject(new Error(i18n.global.t("messages.missing-credentials")));
          return;
        }

        return axios
          .post(configuration.getApiUrl("v1/sessions"), {
            identityName: credentials.identityName,
            password: credentials.password,
            token: credentials.token,
          })
          .then(function (response) {
            if (!response) {
              throw new Error("Argument 'response' may not be undefined.");
            }

            if (!response.data) {
              throw new Error("Argument 'response.data' may not be undefined.");
            }

            const data = response.data;

            switch (data.result) {
              case "Registered": {
                self.register({
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
                reject(
                  new Error(i18n.global.t("exceptions.invalid-credentials"))
                );
                break;
              }
            }

            resolve(response);
          })
          .catch(function (error) {
            reject(error);
          });
      });
    },
    async oauth(oauthData: OAuthData): Promise<AxiosResponse<SessionResponse>> {
      const self = this;

      return new Promise((resolve, reject) => {
        if (!oauthData || !oauthData.state || !oauthData.code) {
          reject(new Error(i18n.global.t("messages.oauth-missing-data")));
          return;
        }

        return axios
          .get<SessionResponse>(
            configuration.getApiUrl(
              `v1/oauth/session/${oauthData.state}/${oauthData.code}`
            )
          )
          .then(function (response: AxiosResponse<SessionResponse>) {
            if (!response) {
              throw new Error("Argument 'response' may not be undefined.");
            }

            if (!response.data) {
              throw new Error("Argument 'response.data' may not be undefined.");
            }

            const data = response.data;

            if (data.result == "Registered") {
              self.register({
                identityName: data.identityName,
                token: data.token,
                permissions: data.permissions,
              });
            }

            resolve(response);
          })
          .catch(function (error) {
            reject(error);
          })
          .finally(() => {
            this.initialized = true;
          });
      });
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
