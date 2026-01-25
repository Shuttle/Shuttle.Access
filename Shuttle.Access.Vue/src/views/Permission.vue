<template>
  <div>
    <form @submit.prevent="submit" class="sv-form">
      <a-title :title="$t('permission')" close-path="/permissions" />
      <v-text-field v-model="state.name" :label="$t('name')" class="mb-2" :error-messages="validation.message('name')">
      </v-text-field>
      <v-text-field v-model="state.description" :label="$t('description')" class="mb-2">
      </v-text-field>
      <div class="sv-strip sv-strip--reverse">
        <v-btn type="submit" :disabled="busy">{{ $t("save") }}</v-btn>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive, type Reactive } from "vue";
import { required } from '@vuelidate/validators';
import { useValidation } from "@/composables/Validation"
import api from "@/api";
import type { RegisterPermission } from "@/access";
import { useSnackbarStore } from "@/stores/snackbar";

const router = useRouter();

const busy: Ref<boolean> = ref(false);

type State = {
  name: string;
  description: string;
}

const state: Reactive<State> = reactive({
  name: "",
  description: "",
});

const rules = computed(() => {
  return {
    name: {
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
    await api.post<RegisterPermission>("v1/permissions", {
      name: state.name,
      description: state.description,
      status: 1
    })

    useSnackbarStore().requestSent();

    router.push("/permissions");
  } finally {
    busy.value = false;
  }
}
</script>
