<template>
  <form @submit.prevent="submit" class="sv-form">
    <sv-title :title="$t('role-json')" close-drawer type="borderless" />
    <v-textarea v-model="state.json" :label="$t('json')" class="mb-2" :error-messages="validation.message('json')"
      rows="10">
    </v-textarea>
    <div class="sv-strip sv-strip--reverse">
      <v-btn type="submit" :disabled="busy">{{ $t("submit") }}</v-btn>
    </div>
  </form>
</template>

<script setup lang="ts">
import { computed, reactive, type Reactive } from "vue";
import { required } from '@vuelidate/validators';
import { useValidation } from "@/composables/Validation"
import api from "@/api";
import { useDrawerStore } from "@/stores/drawer";
import { useSnackbarStore } from "@/stores/snackbar";
import { useAlertStore } from "@/stores/alert";
import { useI18n } from "vue-i18n";

const drawerStore = useDrawerStore()
const alertStore = useAlertStore();
const { t } = useI18n({ useScope: 'global' });

const busy: Ref<boolean> = ref(false);

type State = {
  json: string;
}

const state: Reactive<State> = reactive({
  json: "",
});

const rules = computed(() => {
  return {
    json: {
      required
    },
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
    let json;

    try {
      json = JSON.parse(state.json);
    } catch {
      // ignore
    }

    if (json === null || !Array.isArray(json)) {
      alertStore.add({
        message: t("messages.invalid-json"),
        variant: "error",
        name: "invalid-json"
      })

      return;
    }
    await api.post("v1/roles/bulk", json)

    useSnackbarStore().requestSent();

    drawerStore.close();
  } finally {
    busy.value = false;
  }
}
</script>
