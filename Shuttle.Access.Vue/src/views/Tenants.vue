<template>
  <v-card flat>
    <v-card-title class="sv-card-title">
      <sv-title :title="$t('tenants')" />
      <div class="sv-strip">
        <v-btn :icon="mdiRefresh" size="x-small" @click="refresh"></v-btn>
        <v-text-field v-model="search" density="compact" :label="$t('search')" :prepend-inner-icon="mdiMagnify"
          variant="solo-filled" flat hide-details single-line></v-text-field>
      </div>
    </v-card-title>
    <v-divider></v-divider>
    <v-data-table :items="items" :headers="headers" :mobile="null" mobile-breakpoint="md" v-model:search="search"
      :loading="busy">
      <template v-slot:header.action="">
        <div class="sv-strip" v-if="sessionStore.hasPermission(Permissions.Tenants.Manage)">
          <v-btn :icon="mdiPlus" size="x-small" @click="add"></v-btn>
        </div>
      </template>
      <template v-slot:item.action="{ item }">
        <div class="sv-strip">
          <v-btn :icon="mdiDeleteOutline" size="x-small"
            @click.stop="confirmationStore.show({ item: item, onConfirm: remove })" />
        </div>
      </template>
    </v-data-table>
  </v-card>
  <sv-form-drawer></sv-form-drawer>
</template>

<script setup lang="ts">
import api from "@/api";
import { onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { mdiDeleteOutline, mdiMagnify, mdiPlus, mdiRefresh } from '@mdi/js';
import { useRouter } from "vue-router";
import { useConfirmationStore } from "@/stores/confirmation";
import { useSecureTableHeaders } from "@/composables/SecureTableHeaders";
import Permissions from "@/permissions";
import type { Tenant } from "@/access";
import { useSessionStore } from "@/stores/session";
import { useDrawerStore } from "@/stores/drawer";
import { useSnackbarStore } from "@/stores/snackbar";

const confirmationStore = useConfirmationStore();
const sessionStore = useSessionStore();
const drawerStore = useDrawerStore()
const { t } = useI18n({ useScope: 'global' });
const router = useRouter();

const busy: Ref<boolean> = ref(false);
const items: Ref<Tenant[]> = ref([]);
const search: Ref<string> = ref('')

const headers = useSecureTableHeaders([
  {
    value: "action",
    headerProps: {
      class: "w-1",
    },
    permission: Permissions.Tenants.Manage,
    filterable: false
  },
  {
    title: t("name"),
    value: "name",
  },
  {
    title: t("status"),
    value: "status",
  },
]);

const refresh = async () => {
  busy.value = true;

  try {
    const response = await api.post("v1/tenants/search", {});
    items.value = response.data;
  } finally {
    busy.value = false;
  }
}

const remove = async (item: Tenant) => {
  confirmationStore.close();

  busy.value = true;

  try {
    await api.delete(`v1/tenants/${item.id}`)

    useSnackbarStore().requestSent();

    refresh();
  } finally {
    busy.value = false;
  }
}

const add = () => {
  router.push({ name: "tenant" })
}

onMounted(() => {
  refresh();

  drawerStore.initialize({
    refresh: refresh,
    parentPath: '/tenants',
  })
})
</script>
