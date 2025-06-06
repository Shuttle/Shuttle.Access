<template>
  <form @submit.prevent="signIn" class="sv-form sv-form--sm px-5 pt-6">
    <div v-if="application" class="mb-6">
      <div class="flex flex-row">
        <div v-if="application.svg" v-html="application.svg" class="v-icon__svg w-10 h-10 mb-2">
        </div>
        <div class="text-xl font-bold">{{ application.title }}</div>
      </div>
      <v-divider></v-divider>
      <div>{{ application.description }}</div>
    </div>
    <sv-title :title="$t('sign-in')"></sv-title>
    <div v-if="configuration.isPasswordAuthenticationAllowed()">
      <v-text-field :prepend-icon="`svg:${mdiAccountOutline}`" v-model="state.identityName" :label="$t('identity-name')"
        class="mb-2" :error-messages="validation.message('identityName')">
      </v-text-field>
      <v-text-field :prepend-icon="`svg:${mdiShieldOutline}`" v-model="state.password" :label="$t('password')"
        :icon-end="getPasswordIcon()" icon-end-clickable :append-icon="`svg:${getPasswordIcon()}`"
        @click:append="togglePasswordIcon" :type="getPasswordType()" :error-messages="validation.message('password')">
      </v-text-field>
      <div class="flex justify-end mt-4">
        <v-btn type="submit" :disabled="busy">{{ $t("sign-in") }}</v-btn>
      </div>
      <v-divider v-if="oauthProviders.length > 0" class="mt-4 mb-2"></v-divider>
    </div>
    <div class="flex flex-row justify-start space-x-2" v-if="oauthProviders.length > 0">
      <div v-for="oauthProvider in oauthProviders" v-bind:key="oauthProvider.name" :alt="`${oauthProvider.name} logo`"
        class="cursor-pointer" @click="oauthAuthenticate(oauthProvider.name)">
        <div v-if="oauthProvider.svg" v-html="oauthProvider.svg" class="v-icon__svg w-10 h-10">
        </div>
        <div v-else
          class="rounded-full bg-zinc-800 text-gray-400 h-10 flex justify-center items-center px-4 uppercase font-semibold">
          {{ oauthProvider.name }}</div>
      </div>
    </div>
  </form>
</template>

<script setup lang="ts">
import { mdiAccountOutline, mdiEyeOutline, mdiEyeOffOutline, mdiShieldOutline } from '@mdi/js';
import { computed, reactive, ref } from "vue";
import { required } from '@vuelidate/validators';
import { useValidation } from "@/composables/Validation"
import { useAlertStore } from "@/stores/alert";
import { useSessionStore } from "@/stores/session";
import { useI18n } from "vue-i18n";
import router from "@/router";
import api from "@/api";
import configuration from "@/configuration";

type OAuthProvider = {
  name: string;
  svg: string;
}

type Application = {
  name: string;
  title: string;
  description: string;
  svg?: string;
}

const props = defineProps({
  applicationName: String
})

const { t } = useI18n({ useScope: 'global' });
const alertStore = useAlertStore();
const sessionStore = useSessionStore();

const busy = ref(false);
const oauthProviders = ref<OAuthProvider[]>([]);
const application = ref<Application>();

const state = reactive({
  identityName: "",
  password: ""
});

const rules = computed(() => {
  return {
    identityName: {
      required
    },
    password: {
      required
    }
  }
});

const validation = useValidation(rules, state);

const passwordVisible = ref(false);

const getPasswordIcon = () => {
  return passwordVisible.value ? mdiEyeOutline : mdiEyeOffOutline;
}

const getPasswordType = () => {
  return passwordVisible.value ? "text" : "password";
}

const togglePasswordIcon = () => {
  passwordVisible.value = !passwordVisible.value;
}

const signIn = async () => {
  const errors = await validation.errors();

  if (errors.length) {
    return;
  }

  busy.value = true;

  try {
    const response = await sessionStore.signIn({
      identityName: state.identityName,
      password: state.password,
      applicationName: props.applicationName
    });

    if (response.sessionTokenExchangeUrl) {
      window.location.replace(response.sessionTokenExchangeUrl);

      return;
    }

    router.push({ name: "dashboard" });
  } catch (error: any) {
    alertStore.add({
      message: error.response?.status == 400 ? t("exceptions.invalid-credentials", { reason: error.response?.data ?? t("exceptions.bad-request") }) : error.toString(),
      type: "error",
      name: "sign-in-exception"
    });
  } finally {
    busy.value = false;
  }
}

const oauthAuthenticate = async (name: string) => {
  busy.value = true;

  try {
    const response = await api.get(`v1/oauth/authenticate/${name}${props.applicationName ? `/${props.applicationName}` : ""}`)

    window.location.replace(response?.data?.authorizationUrl);
  } finally {
    busy.value = false;
  }
}

const refreshOAuthProviders = async () => {
  busy.value = true;

  try {
    const response = await api.get("v1/oauth/providers")

    oauthProviders.value = response?.data;
  } finally {
    busy.value = false;
  }
}

const fetchApplication = async () => {
  busy.value = true;

  try {
    const response = await api.get("v1/applications/" + props.applicationName)
    application.value = response?.data;
  } catch (error: any) {
    alertStore.add({
      message: error.toString(),
      type: "error",
      name: "fetch-application-exception"
    });
  } finally {
    busy.value = false;
  }
}

onMounted(async () => {
  if (!configuration.isOk()) {
    return;
  }

  await refreshOAuthProviders();

  if (props.applicationName) {
    await fetchApplication();
  }
})
</script>
