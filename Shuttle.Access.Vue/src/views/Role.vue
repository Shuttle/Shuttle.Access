<template>
    <form @submit.prevent="submit" class="sv-form">
        <a-title :title="$t('role')" close-drawer type="borderless" />
        <v-text-field v-model="state.name" :label="$t('name')" class="mb-2"
            :error-messages="validation.message('name')">
        </v-text-field>
        <div class="sv-strip sv-strip--reverse">
            <v-btn type="submit" :disabled="busy">{{ $t("save") }}</v-btn>
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

const drawerStore = useDrawerStore()

const busy: Ref<boolean> = ref(false);

type State = {
    name: string;
}

const state: Reactive<State> = reactive({
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
        await api.post("v1/roles", {
            name: state.name,
        })

        useSnackbarStore().requestSent();

        drawerStore.close();
    } finally {
        busy.value = false;
    }
}
</script>
