<template>
  <form @submit.prevent="change" class="sv-form sv-form--sm px-5 pt-20">
    <div class="sv-title">{{ $t("change-password") }}</div>
    <v-text-field :prepend-icon="`svg:${mdiAccountOutline}`" v-model="state.identityName" :label="$t('identity-name')"
      class="mb-2" :error-messages="validation.message('identityName')" readonly>
    </v-text-field>
    <v-text-field :prepend-icon="`svg:${mdiShieldOutline}`" v-model="state.password" :label="$t('password')"
      :icon-end="getPasswordIcon()" icon-end-clickable :append-inner-icon="`svg:${getPasswordIcon()}`"
      @click:append-inner="togglePasswordIcon" :type="getPasswordType()" autocomplete="current-password"
      :error-messages="validation.message('password')">
    </v-text-field>
    <div class="flex justify-end mt-4">
      <v-btn type="submit" :disabled="busy">{{ $t("change") }}</v-btn>
    </div>
  </form>
</template>

<script setup lang="ts">
import { mdiAccountOutline, mdiEyeOutline, mdiEyeOffOutline, mdiShieldOutline } from '@mdi/js';
import { computed, onMounted, reactive, ref, type Reactive } from "vue";
import { required } from '@vuelidate/validators';
import { useValidation } from "@/composables/useValidation"
import { useAlertStore } from "@/stores/alert";
import { useSessionStore } from "@/stores/session";
import { useI18n } from "vue-i18n";
import api from "@/api";
import router from "@/router";
import type { ChangePassword } from '@/access';

const props = defineProps({
  id: String
})

const { t } = useI18n({ useScope: 'global' });
const alertStore = useAlertStore();

const busy: Ref<boolean> = ref(false);

type State = {
  identityName: string | undefined;
  password: string;
}

const state: Reactive<State> = reactive({
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

const passwordVisible: Ref<boolean> = ref(false);

const getPasswordIcon = (): string => {
  return passwordVisible.value ? mdiEyeOutline : mdiEyeOffOutline;
}

const getPasswordType = (): string => {
  return passwordVisible.value ? "text" : "password";
}

const togglePasswordIcon = () => {
  passwordVisible.value = !passwordVisible.value;
}

const change = async () => {
  const sessionStore = useSessionStore();
  const errors = await validation.errors();

  if (errors.length) {
    return;
  }

  busy.value = true;

  api.put<ChangePassword>("v1/identities/password/change", {
    id: props.id === "token" ? undefined : props.id,
    token: props.id === "token" ? sessionStore.token : undefined,
    newPassword: state.password
  })
    .then(() => {
      router.push({ name: props.id === "token" ? "dashboard" : "identities" });

      alertStore.add({
        message: t("messages.password-changed"),
        variant: "success",
        name: "password-changed"
      });
    })
    .finally(() => {
      busy.value = false;
    });
}

onMounted(() => {
  if (props.id === "token") {
    const sessionStore = useSessionStore();

    state.identityName = sessionStore.identityName;

    return;
  }

  api.get(`v1/identities/${props.id}`)
    .then(response => {
      state.identityName = response.data.name;
    });
})
</script>
