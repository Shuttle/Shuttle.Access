<template>
    <v-app>
        <div v-if="alertStore.alerts.length" class="m-2 lg:w-1/2 md:w-100 mx-auto">
            <v-alert :type="alert.type" v-bind:key="alert.key" v-for="alert in alertStore.alerts" :text="alert.message" >
                <template v-slot:close>
                    <v-icon :icon="`svg:${mdiCloseCircleOutline}`" @click="closeClicked(alert.name)" />
                </template>
            </v-alert>
        </div>
        <v-main>
            <router-view />
        </v-main>
    </v-app>
</template>

<script lang="ts" setup>
import { mdiCloseCircleOutline } from '@mdi/js';
import { useAlertStore } from "@/stores/alert";

var alertStore = useAlertStore();

const closeClicked = (name: string) => {
    alertStore.remove(name);
}
</script>
