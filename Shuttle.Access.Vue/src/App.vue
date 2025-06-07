<template>
  <v-app>
    <v-layout>
      <div v-if="alertStore.alerts.length"
        class="absolute top-0 left-0 right-0 bottom-0 z-5000 flex flex-col items-center bg-black/25 w-full"
        @click="clearAlerts">
        <div class="mt-6 mb-2 lg:w-1/2 md:w-100 lg:mx-auto md:mx-2">
          <v-alert :type="alert.type" v-bind:key="alert.key" v-for="alert in alertStore.alerts" :text="alert.message"
            class="mb-2">
            <template v-slot:text>
              {{ alert.message }}
            </template>
            <template v-slot:close v-if="alert.dismissable">
              <v-icon :icon="`svg:${mdiCloseCircleOutline}`" @click="closeClicked(alert.name)" />
            </template>
          </v-alert>
        </div>
      </div>
      <Navbar />
      <v-main>
        <div class="p-2">
          <RouterView />
        </div>
        <v-snackbar v-model="snackbarStore.visible" :timeout="snackbarStore.timeout">
          {{ snackbarStore.text }}

          <template v-slot:actions>
            <v-btn color="blue" variant="text" @click="snackbarStore.close()"
              :icon="`svg:${mdiCloseCircleOutline}`"></v-btn>
          </template>
        </v-snackbar>
      </v-main>
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
    </v-layout>
  </v-app>
</template>

<script lang="ts" setup>
import Navbar from "@/components/Navbar.vue";
import { mdiCloseCircleOutline } from '@mdi/js';
import { useAlertStore } from "@/stores/alert";
import { RouterView } from "vue-router";
import { useConfirmationStore } from "@/stores/confirmation";
import { useSnackbarStore } from '@/stores/snackbar'
import configuration from "@/configuration";
import { useI18n } from "vue-i18n";

const alertStore = useAlertStore();
const confirmationStore = useConfirmationStore();
const snackbarStore = useSnackbarStore()

const { t } = useI18n({ useScope: 'global' });

alertStore.initialize();

const clearAlerts = () => {
  alertStore.clear()
}

const closeClicked = (name: string) => {
  alertStore.remove(name);
}

if (!configuration.isOk()) {
  t("configuration-error")
  alertStore.add({
    message: t("exceptions.configuration-error", { error: configuration.getErrorMessage() }),
    type: "error",
    name: "configuration-error",
    dismissable: false,
    expire: false
  });
}
</script>
