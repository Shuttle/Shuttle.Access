<template>
  <v-card flat>
    <v-card-title class="sv-card-title">
      <div class="sv-title">{{ $t("identities") }}</div>
      <div class="sv-strip">
        <v-btn :icon="mdiRefresh" size="small" @click="refresh"></v-btn>
        <v-btn v-if="sessionStore.hasPermission(Permissions.Identities.Manage)" :icon="mdiPlus" size="small"
          @click="add"></v-btn>
        <v-text-field v-model="search" density="compact" :label="$t('search')" :prepend-inner-icon="mdiMagnify"
          variant="solo-filled" flat hide-details single-line></v-text-field>
      </div>
    </v-card-title>
    <v-divider></v-divider>
    <v-data-table :items="items" :headers="headers" :mobile="null" mobile-breakpoint="md" v-model:search="search"
      :loading="busy">
      <template v-slot:item.roles="{ item }">
        <v-btn :icon="mdiAccountGroupOutline" size="x-small" @click="roles(item)" v-tooltip:end="$t('roles')" />
      </template>
      <template v-slot:item.rename="{ item }">
        <v-btn :icon="mdiPencil" size="x-small" @click="rename(item)" v-tooltip:end="$t('rename')" />
      </template>
      <template v-slot:item.password="{ item }">
        <v-btn :icon="mdiShieldOutline" size="x-small" @click="password(item)" v-tooltip:end="$t('password')" />
      </template>
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
import { mdiMagnify, mdiDeleteOutline, mdiPlus, mdiRefresh, mdiPencil, mdiShieldOutline, mdiAccountGroupOutline } from '@mdi/js';
import { useDateFormatter } from "@/composables/useDateFormatter";
import { useSecureTableHeaders } from "@/composables/useSecureTableHeaders";
import { useRouter } from "vue-router";
import { useAlertStore } from "@/stores/alert";
import { useConfirmationStore } from "@/stores/confirmation";
import Permissions from "@/permissions";
import type { Identity } from "@/access";
import type { AxiosResponse } from "axios";
import { useSessionStore } from "@/stores/session";

const confirmationStore = useConfirmationStore();
const sessionStore = useSessionStore();

const { t } = useI18n({ useScope: 'global' });
const router = useRouter();
const busy = ref(false);
const search = ref('')

const headers = useSecureTableHeaders([
  {
    value: "roles",
    headerProps: {
      class: "w-1"
    },
    permission: Permissions.Identities.Manage
  },
  {
    value: "password",
    headerProps: {
      class: "w-1"
    },
    permission: Permissions.Identities.Manage
  },
  {
    value: "rename",
    headerProps: {
      class: "w-1"
    },
    permission: Permissions.Identities.Manage
  },
  {
    value: "remove",
    headerProps: {
      class: "w-1"
    },
    permission: Permissions.Identities.Manage
  },
  {
    title: t("name"),
    value: "name",
  },
  {
    title: t("generated-password"),
    value: "generatedPassword",
  },
  {
    title: t("date-registered"),
    key: "item.dateRegistered",
    value: (item: any) => {
      return useDateFormatter(item.dateRegistered);
    }
  },
  {
    title: t("registered-by"),
    value: "registeredBy",
  },
  {
    title: t("date-activated"),
    key: "item.dateActivated",
    value: (item: any) => {
      return useDateFormatter(item.dateActivated);
    }
  },
]);

const items: Ref<Identity[]> = ref([]);

const refresh = () => {
  busy.value = true;

  api
    .get("v1/identities")
    .then(function (response: AxiosResponse<Identity[]>) {
      if (!response || !response.data) {
        return;
      }

      items.value = response.data;
    })
    .finally(function () {
      busy.value = false;
    });
}

const remove = (item: Identity) => {
  confirmationStore.setIsOpen(false);

  api
    .delete(`v1/identities/${item.id}`)
    .then(function () {
      useAlertStore().requestSent();

      refresh();
    });
}

const add = () => {
  router.push({ name: "identity" })
}

const roles = (item: Identity) => {
  router.push({ name: "identity-roles", params: { id: item.id } });
}

const password = (item: Identity) => {
  router.push({ name: "password", params: { id: item.id } });
}

const rename = (item: Identity) => {
  router.push({ name: "identity-rename", params: { id: item.id } });
}

onMounted(() => {
  refresh();
})
</script>
