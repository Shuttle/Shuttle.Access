import { defineStore } from "pinia";
import { ref, computed } from "vue";
import axios from "axios";
import configuration from "@/configuration";
import { i18n } from "@/i18n";
import type { Credentials, OAuthData, SessionResponse } from "@/access";

export const useSessionStore = defineStore("session", () => {
  const authenticated = ref(false);
  const initialized = ref(false);
  const identityName = ref<string | undefined>("");
  const token = ref<string | undefined>("");
  const permissions = ref<string[]>([]);

  const status = computed(() => (!token.value ? "not-signed-in" : "signed-in"));

  async function initialize() {
    if (initialized.value) return;

    const storedIdentity = localStorage.getItem("shuttle-access.identityName");
    const storedToken = localStorage.getItem("shuttle-access.token");

    if (storedIdentity && storedToken) {
      try {
        return await signIn({
          identityName: storedIdentity,
          token: storedToken,
        });
      } finally {
        initialized.value = true;
      }
    }

    return Promise.resolve();
  }

  function addPermission(permission: string) {
    if (hasPermission(permission)) return;
    permissions.value.push(permission);
  }

  function removePermissions() {
    permissions.value = [];
  }

  function register(session: SessionResponse) {
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

    identityName.value = session.identityName;
    token.value = session.token;

    removePermissions();

    session.permissions.forEach((item) => addPermission(item));

    authenticated.value = true;
  }

  async function signIn(credentials: Credentials): Promise<SessionResponse> {
    if (
      !credentials ||
      !credentials.identityName ||
      !(credentials.password || credentials.token)
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

    if (!response?.data) {
      throw new Error("Invalid response data.");
    }

    const sessionResponse = response.data;

    switch (sessionResponse.result) {
      case "Registered": {
        if (sessionResponse.sessionTokenExchangeUrl) {
          window.location.replace(sessionResponse.sessionTokenExchangeUrl);
          break;
        }

        register(sessionResponse);
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
  }

  async function oauth(oauthData: OAuthData): Promise<SessionResponse> {
    if (!oauthData || !oauthData.state || !oauthData.code) {
      throw new Error(i18n.global.t("messages.oauth-missing-data"));
    }

    const response = await axios.get<SessionResponse>(
      configuration.getApiUrl(
        `v1/oauth/session/${oauthData.state}/${oauthData.code}`,
      ),
    );

    if (!response?.data) {
      throw new Error("Invalid response data.");
    }

    const sessionResponse = response.data;

    if (sessionResponse.result === "Registered") {
      register(sessionResponse);
    }

    initialized.value = true;

    return sessionResponse;
  }

  function signOut() {
    identityName.value = undefined;
    token.value = undefined;

    localStorage.removeItem("shuttle-access.identityName");
    localStorage.removeItem("shuttle-access.token");

    removePermissions();

    authenticated.value = false;
  }

  function hasSession() {
    return !!token.value;
  }

  function hasPermission(permission: string) {
    const required = permission.toLowerCase();
    let result = false;

    permissions.value.forEach((item) => {
      if (result) return;

      if (item.toLowerCase() === required) {
        result = true;
        return;
      }

      if (item.includes("*")) {
        const escaped = item
          .replace(/[.*+?^${}()|[\]\\]/g, "\\$&")
          .replace(/\\\*/g, ".*");

        const regex = new RegExp(`^${escaped}$`, "i");
        if (regex.test(required)) {
          result = true;
        }
      }
    });

    return result;
  }

  return {
    authenticated,
    initialized,
    identityName,
    token,
    permissions,
    status,
    initialize,
    addPermission,
    removePermissions,
    register,
    signIn,
    signOut,
    oauth,
    hasSession,
    hasPermission,
  };
});
