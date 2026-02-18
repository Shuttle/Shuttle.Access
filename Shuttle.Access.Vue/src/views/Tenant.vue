<template>
  <form @submit.prevent="submit" class="sv-form">
    <a-title :title="$t('tenant')" close-drawer type="borderless" />
    <v-text-field v-model="state.id" :label="$t('id')" class="mb-2" clearable>
    </v-text-field>
    <v-text-field v-model="state.name" :label="$t('name')" class="mb-2" :error-messages="validation.message('name')">
    </v-text-field>
    <v-text-field v-model="state.logoSvg" :label="$t('logo-svg')" class="mb-2"
      :error-messages="validation.message('logoSvg')">
    </v-text-field>
    <v-text-field v-model="state.logoUrl" :label="$t('logo-url')" class="mb-2"
      :error-messages="validation.message('logoUrl')">
    </v-text-field>
    <div class="sv-strip sv-strip--reverse">
      <v-btn type="submit" :disabled="busy">{{ $t("save") }}</v-btn>
    </div>
  </form>
</template>

<script setup lang="ts">
import { computed, reactive, type Reactive, ref, type Ref } from "vue";
import { required } from '@vuelidate/validators';
import { useValidation } from "@/composables/Validation"
import api from "@/api";
import { useDrawerStore } from "@/stores/drawer";
import { useSnackbarStore } from "@/stores/snackbar";
import type { Tenant } from "@/access";

const drawerStore = useDrawerStore()

const busy: Ref<boolean> = ref(false);

const state: Reactive<Tenant> = reactive({
  id: "",
  name: "",
  logoSvg: "",
  logoUrl: "",
  status: 1
});

const rules = computed(() => {
  return {
    name: {
      required
    },
    logoSvg: {},
    logoUrl: {}
  }
});

const validation = useValidation(rules, state);

const submit = async () => {
  const errors = await validation.errors();

  if (errors.length) {
    return;
  }

  busy.value = true;

  try {
    await api.post("v1/tenants", state)

    useSnackbarStore().requestSent();

    drawerStore.close();
  } finally {
    busy.value = false;
  }
}
</script>
