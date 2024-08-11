<template>
    <div>
        <form @submit.prevent="submit" class="sv-form sv-form--sm">
            <div class="sv-title">{{ $t("role") }}</div>
            <v-text-field v-model="state.current" :label="$t('name')" class="mb-2" readonly>
            </v-text-field>
            <v-text-field v-model="state.name" :label="$t('new-value')"
                class="mb-2" :error-messages="validation.message('name')">
            </v-text-field>
            <div class="sv-strip sv-strip--reverse">
                <v-btn type="submit" :disabled="busy || same">{{ $t("save") }}</v-btn>
            </div>
        </form>
    </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from "vue";
import { required } from '@vuelidate/validators';
import { useRoute } from 'vue-router';
import { useValidation } from "@/composables/useValidation"
import { useAlertStore } from "@/stores/alert";
import api from "@/api";

const id = ref(useRoute().params.id);

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
        .patch(`v1/roles/${id.value}/name`, {
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
    api.get(`v1/roles/${id.value}`)
        .then(item => {
            state.current = item.data.name;
            state.name = item.data.name;
        });
})
</script>