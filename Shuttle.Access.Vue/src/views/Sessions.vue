<template>
  <v-card flat>
    <v-card-title class="sv-card-title">
      <div class="sv-title">{{ $t("sessions") }}</div>
      <div class="sv-strip">
        <v-btn :icon="mdiRefresh" size="small" @click="refresh"></v-btn>
        <v-btn :icon="mdiPlus" size="small" @click="add"></v-btn>
        <v-text-field v-model="search" density="compact" :label="$t('search')" :prepend-inner-icon="mdiMagnify"
          variant="solo-filled" flat hide-details single-line></v-text-field>
      </div>
    </v-card-title>
    <v-divider></v-divider>
    <v-data-table :items="items" :headers="headers" :mobile="null" mobile-breakpoint="md" v-model:search="search"
      :loading="busy">
      <template v-slot:item.remove="{ item }">
        <v-btn :icon="mdiDeleteOutline" size="x-small" @click="confirmationStore.show(item, remove)"
          v-tooltip:end="$t('remove')" />
      </template>
    </v-data-table>
  </v-card>
</template>

<script setup lang="ts">
import api from "@/api";
import { onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { mdiDeleteOutline, mdiMagnify, mdiPlus, mdiRefresh } from '@mdi/js';
import { useRouter } from "vue-router";
import { useAlertStore } from "@/stores/alert";
import { useConfirmationStore } from "@/stores/confirmation";
import { useSecureTableHeaders } from "@/composables/useSecureTableHeaders";
import Permissions from "@/permissions";
import type { Session } from "@/access";
import type { AxiosResponse } from "axios";

var confirmationStore = useConfirmationStore();

const { t } = useI18n({ useScope: 'global' });
const router = useRouter();
const busy: Ref<boolean> = ref(false);
const search: Ref<string> = ref('')

const headers = useSecureTableHeaders([
  {
    value: "remove",
    headerProps: {
      class: "w-1",
    },
    permission: Permissions.Roles.Manage,
    filterable: false
  },
  {
    title: t("identity"),
    value: "identityName",
  },
]);

const items: Ref<Session[]> = ref([]);

const refresh = () => {
  busy.value = true;

  api
    .post("v1/sessions/search", {})
    .then(function (response: AxiosResponse<Session[]>) {
      if (!response || !response.data) {
        return;
      }

      items.value = response.data;
    })
    .finally(function () {
      busy.value = false;
    });
}

const remove = (item: Session) => {
  confirmationStore.setIsOpen(false);

  busy.value = true;

  api
    .delete(`v1/session/${item.token}`)
    .then(function () {
      useAlertStore().requestSent();

      refresh();
    })
    .finally(() => {
      busy.value = false;
    });
}

const add = () => {
  router.push({ name: "role" })
}

onMounted(() => {
  refresh();
})
</script>
