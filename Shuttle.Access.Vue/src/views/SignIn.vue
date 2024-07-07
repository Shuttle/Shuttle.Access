<template>
    <h1>{{ $t("sign-in") }}</h1>
    <form @submit.prevent="signIn()">
        <div class="title">{{ $t("sign-in") }}</div>
        <v-text-field v-model="state.identityName" :label="$t('identity-name')" class="mb-2" autocomplete="username"
            :error-messages="validation.message('identityName')">
        </v-text-field>
        <v-text-field v-model="state.password" :label="$t('password')" :icon-end="getPasswordIcon()" icon-end-clickable
            :append-inner-icon="`svg:${getPasswordIcon()}`" @click:append-inner="togglePasswordIcon" :type="getPasswordType()"
            autocomplete="current-password" :error-messages="validation.message('password')">
        </v-text-field>
        <div class="flex flex-row justify-end mt-4">
            <v-btn @click="signIn">{{ $t("sign-in") }}</v-btn>
        </div>
    </form>
</template>

<script setup lang="ts">
import { mdiEyeOutline, mdiEyeOffOutline } from '@mdi/js';
import { computed, reactive, ref } from "vue";
import { required } from '@vuelidate/validators';
import { useValidation } from "@/composables/useValidation"
import { useAlertStore } from "@/stores/alert";
import { useSessionStore } from "@/stores/session";
import { useI18n } from "vue-i18n";
import router from "@/router";

const { t } = useI18n({ useScope: 'global' });
const alertStore = useAlertStore();

const state = reactive({
    identityName: "",
    password: ""
});

const rules = computed(() => {
    return {
        identityName: {
            required
        },
        password: {
            required
        }
    }
});

const validation = useValidation(rules, state);

const passwordVisible = ref(false);

const getPasswordIcon = () => {
    return passwordVisible.value ? mdiEyeOutline : mdiEyeOffOutline;
}

const getPasswordType = () => {
    return passwordVisible.value ? "text" : "password";
}

const togglePasswordIcon = () => {
    passwordVisible.value = !passwordVisible.value;
}

const signIn = async () => {
    const sessionStore = useSessionStore();
    const errors = await validation.errors();

    if (errors.length) {
        return;
    }

    sessionStore.signIn({
        identityName: state.identityName,
        password: state.password
    })
        .then(() => {
            router.push({ name: "dashboard" });

            alertStore.remove("session-initialize");
        })
        .catch(error => {
            alertStore.add({
                message: error.response?.status == 400 ? t("exceptions.invalid-credentials") : error.toString(),
                variant: "danger",
                name: "sign-in-exception"
            });
        });
}
</script>