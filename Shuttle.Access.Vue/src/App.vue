<template>
  <v-app class="pt-16">
    <Navbar />
    <div v-if="alertStore.alerts.length" class="my-4 lg:w-1/2 md:w-100 lg:mx-auto md:mx-2">
      <v-alert :type="alert.type" v-bind:key="alert.key" v-for="alert in alertStore.alerts" :text="alert.message"
        class="mb-2">
        <template v-slot:close>
          <v-icon :icon="`svg:${mdiCloseCircleOutline}`" @click="closeClicked(alert.name)" />
        </template>
      </v-alert>
    </div>
    <div class="p-2">
      <Breadcrumb />
      <RouterView v-slot="{ Component, route }">
        <transition name="route" mode="out-in">
          <component :is="Component" :key="route.fullPath"></component>
        </transition>
      </RouterView>
    </div>
    <v-dialog v-model="confirmationStore.isOpen"
      class="flex flex-row items-center justify-end mt-4 lg:w-2/4 md:w-3/4lg:w-1/4 md:w-2/4">
      <v-card :title="$t(confirmationStore.options?.title || 'confirmation-remove.title')">
        <v-card-text>
          {{ $t(confirmationStore.options?.message || "confirmation-remove.message") }}
        </v-card-text>
        <v-spacer></v-spacer>
        <v-card-actions>
          <v-btn :text="$t('yes')" @click="confirmationStore.confirmed()" color="warning" variant="outlined"></v-btn>
          <v-btn :text="$t('cancel')" @click="confirmationStore.close()" color="secondary" variant="flat"></v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-app>
</template>

<script lang="ts" setup>
import Navbar from "@/components/Navbar.vue";
import Breadcrumb from '@/components/Breadcrumb.vue';
import { mdiCloseCircleOutline } from '@mdi/js';
import { useAlertStore } from "@/stores/alert";
import { RouterView } from "vue-router";
import { useConfirmationStore } from "@/stores/confirmation";

const alertStore = useAlertStore();
const confirmationStore = useConfirmationStore();

const closeClicked = (name: string) => {
  alertStore.remove(name);
}
</script>
