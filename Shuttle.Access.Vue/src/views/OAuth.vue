<template>
  <div
    class="lg:w-2/4 md:w-3/4 p-6 mx-auto bg-zinc-800 text-zinc-300 border border-zinc-400 text-lg flex flex-col justify-center items-center">
    <v-progress-circular indeterminate></v-progress-circular>
    <span>{{ $t("signing-in") }}</span>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useAlertStore } from "@/stores/alert";
import { useSessionStore } from "@/stores/session";
import { useI18n } from "vue-i18n";
import router from "@/router";
import { useRoute } from 'vue-router';

const { t } = useI18n({ useScope: 'global' });

const alertStore = useAlertStore();

const busy = ref(false);

onMounted(async () => {
  const sessionStore = useSessionStore();
  const route = useRoute();

  if (!!route.query.error) {
    alertStore.add({
      message: t("exceptions.oauth-error", { error: route.query.error_description }),
      type: "error",
      name: "oauth-error"
    });

    router.push({ name: "sign-in" });

    return;
  }

  const state = (route.query.state?.toString() || "");
  const code = (route.query.code?.toString() || "");

  try {
    const response = await sessionStore.oauth({
      state: state,
      code: code
    });

    const params = { identityName: response.identityName };

    if (response.sessionTokenExchangeUrl) {
      window.location.replace(response.sessionTokenExchangeUrl);

      return;
    }

    if (response.result === "UnknownIdentity") {
      alertStore.add({
        message: response.registrationRequested ? t("messages.oauth-unknown-identity-registered", params) : t("messages.oauth-unknown-identity", params),
        type: "error",
        name: "oauth-unknown-identity"
      });

      router.push({ name: "sign-in" });

      return;
    }

    router.push({ name: "dashboard" });
  } catch (error: any) {
    alertStore.add({
      message: error.response?.status == 400 ? t("exceptions.invalid-credentials") : error.toString(),
      type: "error",
      name: "sign-in-exception"
    });

    router.push({ name: "sign-in" });
  } finally {
    busy.value = false;
  };
})

</script>
