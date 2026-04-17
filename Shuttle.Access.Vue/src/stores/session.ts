import { defineStore } from "pinia";
import { ref, computed } from "vue";
import axios from "axios";
import configuration from "@/configuration";
import { i18n } from "@/i18n";
import type {
  Credentials,
  OAuthData,
  Session,
  SessionResponse,
  Tenant,
} from "@/access";

export const useSessionStore = defineStore("session", () => {
  const isAuthenticated = ref(false);
  const isInitialized = ref(false);
  const identityName = ref<string | null>();
  const token = ref<string | null>();
  const tenantId = ref<string | null>();
  const permissions = ref<string[]>([]);
  const tenants = ref<Tenant[]>([]);

  const status = computed(() => {
    return !token.value ? "not-signed-in" : "signed-in";
  });

  const initialize = async () => {
    if (isInitialized.value) {
      return;
    }

    identityName.value = localStorage.getItem("shuttle-access.identityName");
    token.value = localStorage.getItem("shuttle-access.token");

    try {
      if (identityName.value && token.value) {
        return await signIn({
          identityName: identityName.value,
          token: token.value,
        });
      }
    } finally {
      isInitialized.value = true;
    }
  };

  const addPermission = (permission: string) => {
    if (hasPermission(permission)) {
      return;
    }

    permissions.value.push(permission);
  };

  const removePermissions = () => {
    permissions.value = [];
  };

  const register = (sessionResponse: SessionResponse) => {
    if (
      !sessionResponse ||
      !sessionResponse.session ||
      (!sessionResponse.token && sessionResponse.result !== "Renewed")
    ) {
      throw Error(i18n.global.t("messages.invalid-session"));
    }

    localStorage.setItem(
      "shuttle-access.identityName",
      sessionResponse.session.identityName,
    );

    if (sessionResponse.token) {
      localStorage.setItem("shuttle-access.token", sessionResponse.token);
      token.value = sessionResponse.token;
    }

    identityName.value = sessionResponse.session.identityName;
    tenantId.value = sessionResponse.session.tenantId;
    tenants.value = sessionResponse.tenants;

    removePermissions();
    sessionResponse.session.permissions.forEach((item) =>
      addPermission(item.name),
    );

    isAuthenticated.value = true;
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

    if (sessionResponse.session) {
      register(sessionResponse);
    }

    return sessionResponse;
  };

  const sessionTenantSelected = (session: Session) => {
    if (!session) {
      throw Error(i18n.global.t("messages.invalid-session"));
    }

    tenantId.value = session.tenantId;

    removePermissions();
    session.permissions.forEach((item) => addPermission(item.name));
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
      if (result) {
        return;
      }

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
    tenantSelected: sessionTenantSelected,
  };
});
