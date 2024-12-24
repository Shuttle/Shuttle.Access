<template>
  <form @submit.prevent="submit" class="sv-form sv-form--sm px-5 pt-20">
    <div class="sv-title">{{ $t("identity") }}</div>
    <v-text-field :prepend-icon="`svg:${mdiAccountOutline}`" v-model="state.identityName" :label="$t('identity-name')"
      class="mb-2" :error-messages="validation.message('identityName')" autocomplete="off">
    </v-text-field>
    <v-text-field :prepend-icon="`svg:${mdiShieldOutline}`" v-model="state.password" :label="$t('password')"
      :icon-end="getPasswordIcon()" icon-end-clickable :append-icon="`svg:${getPasswordIcon()}`"
      @click:append="togglePasswordIcon" :type="getPasswordType()" :error-messages="validation.message('password')"
      autocomplete="off">
    </v-text-field>
    <div class="flex justify-end mt-4">
      <v-btn type="submit" :disabled="busy">{{ $t("save") }}</v-btn>
    </div>
  </form>
</template>

<script setup lang="ts">
import { mdiAccountOutline, mdiEyeOutline, mdiEyeOffOutline, mdiShieldOutline } from '@mdi/js';
import { computed, reactive, ref } from "vue";
import { required } from '@vuelidate/validators';
import { useValidation } from "@/composables/useValidation"
import { useAlertStore } from "@/stores/alert";
import api from "@/api";
import type { RegisterIdentity } from '@/access';

const busy: Ref<boolean> = ref(false);

const state = reactive({
  identityName: "",
  password: ""
});

const rules = computed(() => {
  return {
    identityName: {
      required
    },
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

const submit = async () => {
  const errors = await validation.errors();

  if (errors.length) {
    return;
  }

  busy.value = true;

  api
    .post < RegisterIdentity > ("v1/identities", {
      name: state.identityName,
      password: state.password,
      system: "system://access"
    })
      .then(function () {
        useAlertStore().requestSent();
      })
      .finally(() => {
        busy.value = false;
      });
}
</script>
