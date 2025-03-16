<template>
    <div>
        <form @submit.prevent="submit" class="sv-form sv-form--sm">
            <sv-title :title="$t('permission')" close-path="/permissions" />
            <v-text-field v-model="state.name" :label="$t('name')" class="mb-2"
                :error-messages="validation.message('name')">
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
import { useAlertStore } from "@/stores/alert";
import api from "@/api";
import type { RegisterPermission } from "@/access";

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

    api
        .post<RegisterPermission>("v1/permissions", {
            name: state.name,
            status: 1
        })
        .then(function () {
            useAlertStore().requestSent();
        })
        .finally(() => {
            busy.value = false;
        });
}
</script>
