import { defineStore } from "pinia";
import { ref, computed } from "vue";
import axios from "axios";
import configuration from "@/configuration";
import { i18n } from "@/i18n";
import type { Credentials, OAuthData, SessionResponse, Tenant } from "@/access";

export const useSessionStore = defineStore("session", () => {
  const isAuthenticated = ref(false);
  const isInitialized = ref(false);
  const identityName = ref<string | undefined>("");
  const token = ref<string | undefined>("");
  const tenantId = ref<string | undefined>("");
  const permissions = ref<string[]>([]);
  const tenants = ref<Tenant[] | undefined>([]);
  const status = computed(() => (!token.value ? "not-signed-in" : "signed-in"));

  const initialize = async () => {
    if (isInitialized.value) return;
    const storedIdentity = localStorage.getItem("shuttle-access.identityName");
    const storedToken = localStorage.getItem("shuttle-access.token");
    if (storedIdentity && storedToken) {
      try {
        return await signIn({
          identityName: storedIdentity,
          token: storedToken,
        });
      } finally {
        isInitialized.value = true;
      }
    }
    return Promise.resolve();
  };

  const addPermission = (permission: string) => {
    if (hasPermission(permission)) return;
    permissions.value.push(permission);
  };

  const removePermissions = () => {
    permissions.value = [];
  };

  const register = (sessionResponse: SessionResponse) => {
    if (
      !sessionResponse ||
      !sessionResponse.identityId ||
      !sessionResponse.identityName ||
      !sessionResponse.token
    ) {
      throw Error(i18n.global.t("messages.invalid-session"));
    }

    localStorage.setItem(
      "shuttle-access.identityName",
      sessionResponse.identityName,
    );
    localStorage.setItem("shuttle-access.token", sessionResponse.token);
    identityName.value = sessionResponse.identityName;
    token.value = sessionResponse.token;
    tenantId.value = sessionResponse.tenantId;
    removePermissions();
    sessionResponse.permissions.forEach((item) => addPermission(item));
    isAuthenticated.value = true;
  };

  const tenantSelected = (sessionResponse: SessionResponse) => {
    if (!sessionResponse) {
      throw Error(i18n.global.t("messages.invalid-session"));
    }

    tenantId.value = sessionResponse.tenantId;
    removePermissions();
    sessionResponse.permissions.forEach((item) => addPermission(item));
  };

  const signIn = async (credentials: Credentials): Promise<SessionResponse> => {
    if (
      !credentials ||
      !credentials.identityName ||
      !(credentials.password || credentials.token)
    ) {
      throw new Error(i18n.global.t("messages.missing-credentials"));
    }
    const { data: sessionResponse } = await axios.post<SessionResponse>(
      configuration.getApiUrl("v1/sessions"),
      {
        identityName: credentials.identityName,
        password: credentials.password,
        token: credentials.token,
      },
    );
    if (!sessionResponse) {
      throw new Error("Invalid response data.");
    }

    register(sessionResponse);

    return sessionResponse;
  };

  const oauth = async (oauthData: OAuthData): Promise<SessionResponse> => {
    if (!oauthData || !oauthData.state || !oauthData.code) {
      throw new Error(i18n.global.t("messages.oauth-missing-data"));
    }
    const { data: sessionResponse } = await axios.get<SessionResponse>(
      configuration.getApiUrl(
        `v1/oauth/session/${oauthData.state}/${oauthData.code}`,
      ),
    );
    if (!sessionResponse) {
      throw new Error("Invalid response data.");
    }

    register(sessionResponse);

    tenants.value = sessionResponse.tenants;
    isInitialized.value = true;
    return sessionResponse;
  };

  const signOut = () => {
    identityName.value = undefined;
    token.value = undefined;
    tenantId.value = undefined;
    localStorage.removeItem("shuttle-access.identityName");
    localStorage.removeItem("shuttle-access.token");
    removePermissions();
    isAuthenticated.value = false;
  };

  const hasSession = () => {
    return !!token.value;
  };

  const hasPermission = (permission: string) => {
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
  };

  return {
    isAuthenticated,
    isInitialized,
    identityName,
    token,
    tenantId,
    permissions,
    status,
    tenants,
    initialize,
    addPermission,
    removePermissions,
    register,
    signIn,
    signOut,
    oauth,
    hasSession,
    hasPermission,
    tenantSelected,
  };
});
