import { defineStore } from "pinia";
import axios from "axios";
import configuration from "@/configuration";

export const messages = {
  missingCredentials: "Incomplete credentials specified.",
  signInFailure: "Invalid credentials.",
  invalidSession: "Invalid session.",
};

export interface Permission {
  type: string;
  permission: string;
}

export interface Session {
  identityName: string;
  token: string;
  permissions: string[];
}

export interface Credentials {
  identityName: string;
  token?: string;
  password?: string;
}

export interface SessionStoreState {
  authenticated: boolean;
  initialized: boolean;
  identityName?: string;
  token?: string;
  permissions: Permission[];
}

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
      return !this.token
        ? "not-signed-in"
        : "signed-in";
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
            self.initialized = true;

            return response;
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
        throw Error(messages.invalidSession);
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
    async signIn(credentials: Credentials) {
      const self = this;

      return new Promise((resolve, reject) => {
        if (
          !credentials ||
          !credentials.identityName ||
          !(!!credentials.password || !!credentials.token)
        ) {
          reject(new Error(messages.missingCredentials));
          return;
        }

        return axios
          .post(configuration.getApiUrl("sessions"), {
            identityName: credentials.identityName,
            password: credentials.password,
            token: credentials.token,
          })
          .then(function (response) {
            if (!response) {
              throw new Error("Argument 'response' may not be undefined.");
            }

            if (!response.data){
                throw new Error("Argument 'response.data' may not be undefined.");
            }

            const data = response.data;

            self.register({
              identityName: credentials.identityName,
              token: data.token,
              permissions: data.permissions,
            });

            resolve(response);
          })
          .catch(function (error) {
            reject(error);
          });
      });
    },
    signOut() {
      this.identityName = undefined;
      this.token = undefined;

      localStorage.removeItem("shuttle-access.identityName");
      localStorage.removeItem("shuttle-access.token");

      this.removePermissions("identity");
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
