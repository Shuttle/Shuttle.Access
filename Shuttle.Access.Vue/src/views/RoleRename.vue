<template>
  <form @submit.prevent="submit" class="sv-form">
    <sv-title :title="$t('role')" close-path="/roles" type="borderless" />
    <v-text-field v-model="state.current" :label="$t('name')" class="mb-2" readonly>
    </v-text-field>
    <v-text-field v-model="state.name" :label="$t('new-value')" class="mb-2"
      :error-messages="validation.message('name')">
    </v-text-field>
    <div class="sv-strip sv-strip--reverse">
      <v-btn type="submit" :disabled="busy || same">{{ $t("save") }}</v-btn>
    </div>
  </form>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref, type Reactive } from "vue";
import { required } from '@vuelidate/validators';
import { useValidation } from "@/composables/Validation"
import { useAlertStore } from "@/stores/alert";
import api from "@/api";

const router = useRouter();

const props = defineProps<{
  id: string;
}>();

const busy: Ref<boolean> = ref(false);

const same: ComputedRef<boolean> = computed(() => {
  return state.current === state.name;
})

type State = {
  current: string;
  name: string;
}

const state: Reactive<State> = reactive({
  current: "",
  name: "",
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
    await api.patch(`v1/roles/${props.id}/name`, {
      name: state.name,
    })

    useAlertStore().requestSent();

    router.push("/roles");
  } finally {
    busy.value = false;
  }
}

onMounted(() => {
  api.get(`v1/roles/${props.id}`)
    .then(item => {
      state.current = item.data.name;
      state.name = item.data.name;
    });
})
</script>
