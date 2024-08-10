<template>
    <v-app class="pt-14">
        <Navbar />
        <div v-if="alertStore.alerts.length" class="my-4 lg:w-1/2 md:w-100 lg:mx-auto md:mx-2">
            <v-alert :type="alert.type" v-bind:key="alert.key" v-for="alert in alertStore.alerts" :text="alert.message" >
                <template v-slot:close>
                    <v-icon :icon="`svg:${mdiCloseCircleOutline}`" @click="closeClicked(alert.name)" />
                </template>
            </v-alert>
        </div>
        <div class="p-4">
        <RouterView v-slot="{ Component, route }">
            <transition name="route" mode="out-in">
                <component :is="Component" :key="route.fullPath"></component>
            </transition>
        </RouterView>
    </div>
    </v-app>
</template>

<script lang="ts" setup>
import Navbar from "@/components/Navbar.vue";
import { mdiCloseCircleOutline } from '@mdi/js';
import { useAlertStore } from "@/stores/alert";
import { RouterView } from "vue-router";

var alertStore = useAlertStore();

const closeClicked = (name: string) => {
    alertStore.remove(name);
}
</script>

<style>
@import "@/assets/base.css";
</style>
