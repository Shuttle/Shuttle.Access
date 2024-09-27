<template>
    <form @submit.prevent="signIn" class="sv-form sv-form--sm px-5 pt-6">
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
        <v-divider v-if="oauthProviders.length > 0" class="mt-4 mb-2"></v-divider>
        <div class="flex flex-row justify-start space-x-2" v-if="oauthProviders.length > 0">
            <div v-for="oauthProvider in oauthProviders" v-bind:key="oauthProvider.name"
                :alt="`${oauthProvider.name} logo`" class="cursor-pointer"
                @click="oauthAuthenticate(oauthProvider.name)">
                <div v-if="oauthProvider.svg" v-html="oauthProvider.svg" class="oauth-provider w-10 h-10">
                </div>
                <div v-else class="rounded-full bg-zinc-800 text-gray-400 h-10 flex justify-center items-center px-4 uppercase font-semibold">{{ oauthProvider.name }}</div>
            </div>
        </div>
    </form>
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
import api from "@/api";

interface OAuthProvider {
    name: string;
    svg: string;
}

const { t } = useI18n({ useScope: 'global' });
const alertStore = useAlertStore();

const busy = ref(false);
const oauthProviders = ref<OAuthProvider[]>([]);

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

const oauthAuthenticate = (name: string) => {
    busy.value = true;

    api
        .get("v1/oauth/authenticate/" + name)
        .then((response) => {
            window.location.replace(response?.data?.authorizationUrl);
        })
        .finally(() => {
            busy.value = false;
        });
}

const refreshOAuthProviders = () => {
    busy.value = true;

    api
        .get("v1/oauth/providers")
        .then(async (response) => {
            oauthProviders.value = response?.data;
        })
        .finally(function () {
            busy.value = false;
        });
}

onMounted(() => {
    refreshOAuthProviders();
})

</script>