<template>
    <div>
        <form @submit.prevent="submit" class="sv-form sv-form--sm">
            <div class="sv-title">{{ $t("role") }}</div>
            <v-text-field v-model="state.name" :label="$t('name')" class="mb-2"
                :error-messages="validation.message('name')">
            </v-text-field>
            <div class="sv-strip sv-strip--reverse">
                <v-btn type="submit" :disabled="busy">{{ $t("save") }}</v-btn>
            </div>
        </form>
    </div>
</template>

<script setup>
import { computed, reactive } from "vue";
import { required } from '@vuelidate/validators';
import { useValidation } from "@/composables/useValidation"
import { useAlertStore } from "@/stores/alert";
import api from "@/api";

const busy = ref(false);

const state = reactive({
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
        .post("v1/roles", {
            name: state.name,
        })
        .then(function () {
            useAlertStore().requestSent();
        })
        .finally(() => {
            busy.value = false;
        });
}
</script>