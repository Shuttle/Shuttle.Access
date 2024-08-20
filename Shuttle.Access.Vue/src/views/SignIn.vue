<template>
    <form @submit.prevent="signIn" class="sv-form sv-form--sm px-5 pt-20">
        <div class="sv-title">{{ $t("sign-in") }}</div>
        <v-text-field :prepend-icon="`svg:${mdiAccountOutline}`" v-model="state.identityName"
            :label="$t('identity-name')" class="mb-2" autocomplete="username"
            :error-messages="validation.message('identityName')">
        </v-text-field>
        <v-text-field :prepend-icon="`svg:${mdiShieldOutline}`" v-model="state.password" :label="$t('password')"
            :icon-end="getPasswordIcon()" icon-end-clickable :append-inner-icon="`svg:${getPasswordIcon()}`"
            @click:append-inner="togglePasswordIcon" :type="getPasswordType()" autocomplete="current-password"
            :error-messages="validation.message('password')">
        </v-text-field>
        <div class="flex justify-end mt-4">
            <v-btn type="submit" :disabled="busy">{{ $t("sign-in") }}</v-btn>
        </div>
    </form>
    <div class="mt-5">
        <div class="sv-title">{{ $t("login-provider") }}</div>
        <img src="@/assets/github.logo.svg" alt="GitHub logo" class="oauth-provider" @click="oauth('github')" />
    </div>
</template>

<script setup lang="ts">
import { mdiAccountOutline, mdiEyeOutline, mdiEyeOffOutline, mdiShieldOutline } from '@mdi/js';
import { computed, reactive, ref } from "vue";
import { required } from '@vuelidate/validators';
import { useValidation } from "@/composables/useValidation"
import { useAlertStore } from "@/stores/alert";
import { useSessionStore } from "@/stores/session";
import { useI18n } from "vue-i18n";
import router from "@/router";
import configuration from '@/configuration';

const { t } = useI18n({ useScope: 'global' });
const alertStore = useAlertStore();

const busy = ref(false);

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

    busy.value = true;

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
                type: "error",
                name: "sign-in-exception"
            });
        })
        .finally(() => {
            busy.value = false;
        });
}

const oauth = (name: string) => {
      window.location.href = configuration.getOAuthUrl(name);
    },

</script>