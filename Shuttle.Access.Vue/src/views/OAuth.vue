<template>
    <div class="lg:w-2/4 md:w-3/4 p-6 mx-auto border border-stone-900 bg-zinc-500 text-zinc-300 text-lg h-12 flex flex-col justify-center items-center">
        <v-progress-circular indeterminate></v-progress-circular>
        <span>{{ $t("signing-in") }}</span>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useAlertStore } from "@/stores/alert";
import { useSessionStore } from "@/stores/session";
import { useI18n } from "vue-i18n";
import router from "@/router";
import { useRoute } from 'vue-router';

const { t } = useI18n({ useScope: 'global' });

const alertStore = useAlertStore();

const busy = ref(false);

onMounted(() => {
    const sessionStore = useSessionStore();
    const route = useRoute();

    const state = (route.query.state?.toString() || "");
    var code = (route.query.code?.toString() || "");

    sessionStore.oauth({
        state: state,
        code: code
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

            router.push({ name: "sign-in" });
        })
        .finally(() => {
            busy.value = false;
        });
})

</script>