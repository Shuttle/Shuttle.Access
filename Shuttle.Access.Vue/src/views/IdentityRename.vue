<template>
  <form @submit.prevent="submit" class="sv-form sv-form--sm">
    <sv-title :title="$t('identity')" close-path="/identities" />
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
import { computed, onMounted, reactive, ref } from "vue";
import { required } from '@vuelidate/validators';
import { useValidation } from "@/composables/useValidation"
import { useAlertStore } from "@/stores/alert";
import api from "@/api";

const props = defineProps({
  id: String
})

const busy = ref(false);

const same = computed(() => {
  return state.current === state.name;
})

const state = reactive({
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

  api
    .patch(`v1/identities/${props.id}/name`, {
      name: state.name,
    })
    .then(function () {
      useAlertStore().requestSent();
    })
    .finally(() => {
      busy.value = false;
    });
}

onMounted(() => {
  api.get(`v1/identities/${props.id}`)
    .then(item => {
      state.current = item.data.name;
      state.name = item.data.name;
    });
})
</script>
