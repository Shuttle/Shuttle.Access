<template>
    <div>
        <form @submit.prevent="submit" class="sv-form sv-form--sm px-5 pt-20">
            <div class="sv-title">{{ $t("permission") }}</div>
            <v-text-field v-model="state.name" :label="$t('name')"
                class="mb-2" :error-messages="validation.message('name')">
            </v-text-field>
            <div class="sv-strip sv-strip--reverse">
                <v-btn type="submit">{{ $t("save") }}</v-btn>
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

    api
        .post("v1/permissions", {
            name: state.name,
        })
        .then(function () {
            useAlertStore().requestSent();
        });
}
</script>