import { defineStore } from "pinia";
import { ref, computed } from "vue";
import axios from "axios";
import configuration from "@/configuration";
import { i18n } from "@/i18n";
import type {
  Credentials,
  OAuthData,
  SessionPermission,
  SessionResponse,
  Tenant,
} from "@/access";

export const useSessionStore = defineStore("session", () => {
  const isAuthenticated = ref(false);
  const isInitialized = ref(false);
  const identityName = ref<string | null>();
  const token = ref<string | null>();
  const tenantId = ref<string | null>();
  const tenantName = ref<string | null>();
  const sessionPermissions = ref<SessionPermission[]>([]);
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
    tenants.value = sessionResponse.tenants;

    if (tenants.value.length == 1) {
      selectTenantId(tenants.value[0].id);
    }

    sessionPermissions.value = sessionResponse.session.permissions;

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

  const getTenantName = (id: string) => {
    return tenants.value.find((t) => t.id === id)?.name ?? null;
  };

  const selectTenantId = (id: string) => {
    tenantId.value = id;
    tenantName.value = getTenantName(id);
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

    sessionPermissions.value = [];

    isAuthenticated.value = false;
  };

  const hasSession = () => {
    return !!token.value;
  };

  const activePermissions = computed(() => {
    return sessionPermissions.value.filter(
      (item) =>
        item.tenantId === tenantId.value &&
        (!item.name.startsWith("access://tenants/") ||
          tenantId.value === configuration.systemTenantId),
    );
  });

  const hasPermission = (permission: string) => {
    if (!tenantId.value) {
      return false;
    }

    const required = permission.toLowerCase();

    let result = false;

    activePermissions.value.forEach((item) => {
      if (result) {
        return;
      }

      if (item.name.toLowerCase() === required) {
        result = true;
        return;
      }

      if (item.name.includes("*")) {
        const escaped = item.name
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

  const systemTenantActive = computed(() => {
    return tenantId.value === configuration.systemTenantId;
  });

  return {
    isAuthenticated,
    isInitialized,
    identityName,
    token,
    tenantId,
    tenantName,
    activePermissions,
    sessionPermissions,
    status,
    tenants,
    systemTenantActive,
    initialize,
    register,
    signIn,
    signOut,
    oauth,
    hasSession,
    hasPermission,
    selectTenantId,
    getTenantName,
  };
});
