<template>
  <v-card flat>
    <v-card-title class="sv-card-title">
      <a-title :title="$t('identities')" />
      <div class="sv-strip">
        <v-btn :icon="mdiRefresh" size="x-small" @click="refresh"></v-btn>
        <v-text-field v-model="search" density="compact" :label="$t('search')" :prepend-inner-icon="mdiMagnify"
          variant="solo-filled" flat hide-details single-line></v-text-field>
      </div>
    </v-card-title>
    <v-divider></v-divider>
    <a-data-table :items="items" :headers="headers" :mobile="null" mobile-breakpoint="md" v-model:search="search"
      :loading="busy" show-expand v-model:expanded="expanded" item-value="name" expand-on-click>
      <template v-slot:header.action="">
        <v-btn v-if="sessionStore.hasPermission(Permissions.Identities.Manage)" :icon="mdiPlus" size="x-small"
          @click="add"></v-btn>
      </template>
      <template v-slot:item.action="{ item }">
        <div class="sv-strip">
          <v-btn :icon="mdiAccountGroupOutline" size="x-small" @click.stop="roles(item)" />
          <v-btn :icon="mdiKey" size="x-small" @click.stop="password(item)" />
          <v-btn :icon="mdiDelete" size="x-small"
            @click.stop="confirmationStore.show({ item: item, onConfirm: remove })" />
        </div>
      </template>
      <template v-slot:item.name="{ item }">
        <div class="flex items-center">
          <div class="flex-grow">{{ item.name }}</div>
          <v-btn :icon="mdiPencil" size="x-small" @click.stop="rename(item)" class="flex-none" />
        </div>
      </template>
      <template v-slot:item.description="{ item }">
        <div class="flex items-center">
          <div class="flex-grow">{{ item.description }}</div>
          <v-btn :icon="mdiPencil" size="x-small" @click.stop="description(item)" class="flex-none" />
        </div>
      </template>
      <template #expanded-row="{ columns, item }">
        <tr>
          <td :colspan="columns.length">
            <div class="sv-table-container">
              <a-data-table :items="item.roles" :headers="roleHeaders" :mobile="null" mobile-breakpoint="md">
              </a-data-table>
            </div>
          </td>
        </tr>
      </template>
    </a-data-table>
  </v-card>
  <a-drawer></a-drawer>
</template>

<script setup lang="ts">
import api from "@/api";
import { onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { mdiMagnify, mdiDelete, mdiPlus, mdiRefresh, mdiPencil, mdiAccountGroupOutline, mdiKey } from '@mdi/js';
import { useDateFormatter } from "@/composables/DateFormatter";
import { useSecureTableHeaders } from "@/composables/SecureTableHeaders";
import { useRouter } from "vue-router";
import { useConfirmationStore } from "@/stores/confirmation";
import Permissions from "@/permissions";
import type { Identity } from "@/access";
import { useSessionStore } from "@/stores/session";
import { useDrawerStore } from "@/stores/drawer";
import { useSnackbarStore } from "@/stores/snackbar";

const confirmationStore = useConfirmationStore();
const sessionStore = useSessionStore();
const drawerStore = useDrawerStore()
const { t } = useI18n({ useScope: 'global' });
const router = useRouter();

const busy = ref(false);
const search = ref('')
const expanded: Ref<string[]> = ref([])

const headers = useSecureTableHeaders([
  {
    value: "action",
    headerProps: {
      class: "w-1"
    },
    permission: Permissions.Identities.Manage
  },
  {
    headerProps: {
      class: "w-96",
    },
    title: t("name"),
    value: "name",
  },
  {
    headerProps: {
      class: "w-96",
    },
    title: t("description"),
    value: "description",
  },
  {
    title: t("generated-password"),
    value: "generatedPassword",
  },
  {
    title: t("date-registered"),
    key: "item.dateRegistered",
    value: (item: any) => {
      return useDateFormatter(item.dateRegistered).dateTimeMilliseconds();
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
      return useDateFormatter(item.dateActivated).dateTimeMilliseconds();
    }
  },
]);

const roleHeaders = useSecureTableHeaders([
  {
    title: t("role"),
    value: "name"
  },
]);


const items: Ref<Identity[]> = ref([]);

const refresh = async () => {
  busy.value = true;

  try {
    const response = await api.post("v1/identities/search", {
      shouldIncludeRoles: true
    });

    if (!response || !response.data) {
      return;
    }

    items.value = response.data;
  } finally {
    busy.value = false;
  }
}

const remove = async (item: Identity) => {
  confirmationStore.close();

  busy.value = true;

  try {
    await api.delete(`v1/identities/${item.id}`);

    useSnackbarStore().requestSent();

    refresh();
  } finally {
    busy.value = false;
  }
}

const add = () => {
  router.push({ name: "identity" })
}

const roles = (item: Identity) => {
  router.push({ name: "identity-roles", params: { id: item.id } });
}

const password = (item: Identity) => {
  router.push({ name: "identity-password", params: { id: item.id } });
}

const rename = (item: Identity) => {
  router.push({ name: "identity-rename", params: { id: item.id } });
}

const description = (item: Identity) => {
  router.push({ name: "identity-description", params: { id: item.id } });
}

onMounted(() => {
  refresh();

  drawerStore.initialize({
    refresh: refresh,
    parentPath: '/identities',
  })
})
</script>
