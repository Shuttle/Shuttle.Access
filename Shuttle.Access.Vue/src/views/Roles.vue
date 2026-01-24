<template>
  <v-card flat>
    <v-card-title class="sv-card-title">
      <a-title :title="$t('roles')" />
      <div class="sv-strip">
        <v-btn :icon="mdiRefresh" size="x-small" @click="refresh"></v-btn>
        <v-text-field v-model="search" density="compact" :label="$t('search')" :prepend-inner-icon="mdiMagnify"
          variant="solo-filled" flat hide-details single-line></v-text-field>
      </div>
    </v-card-title>
    <v-divider></v-divider>
    <a-data-table :items="items" :headers="headers" :mobile="null" mobile-breakpoint="md" v-model:search="search"
      :loading="busy" show-expand v-model:expanded="expanded" expand-on-click show-select v-model="selected">
      <template v-slot:header.action="">
        <div class="sv-strip" v-if="sessionStore.hasPermission(Permissions.Roles.Manage)">
          <v-btn :icon="mdiPlus" size="x-small" @click="add"></v-btn>
          <v-btn v-if="false" :icon="mdiCodeJson" size="x-small" @click="json"></v-btn>
          <v-btn :icon="mdiUpload" size="x-small" @click="upload"></v-btn>
          <v-btn :icon="mdiDownload" size="x-small" @click="download" v-if="selected.length"></v-btn>
        </div>
      </template>
      <template v-slot:item.action="{ item }">
        <div class="sv-strip">
          <v-btn :icon="mdiShield" size="x-small" @click.stop="permissions(item)" />
          <v-btn :icon="mdiPencil" size="x-small" @click.stop="rename(item)" />
          <v-btn :icon="mdiDelete" size="x-small"
            @click.stop="confirmationStore.show({ item: item, onConfirm: remove })" />
        </div>
      </template>
      <template #expanded-row="{ columns, item }">
        <tr>
          <td :colspan="columns.length">
            <div class="sv-table-container">
              <a-data-table :items="item.permissions" :headers="permissionHeaders" :mobile="null"
                mobile-breakpoint="md">
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
import { mdiDelete, mdiDownload, mdiMagnify, mdiPlus, mdiRefresh, mdiPencil, mdiCodeJson, mdiUpload, mdiShield } from '@mdi/js';
import { useRouter } from "vue-router";
import { useConfirmationStore } from "@/stores/confirmation";
import { useSecureTableHeaders } from "@/composables/SecureTableHeaders";
import Permissions from "@/permissions";
import type { Permission, Role } from "@/access";
import { useSessionStore } from "@/stores/session";
import { usePermissionStatuses } from "@/composables/Data";
import { useDrawerStore } from "@/stores/drawer";
import { useSnackbarStore } from "@/stores/snackbar";

const confirmationStore = useConfirmationStore();
const sessionStore = useSessionStore();
const drawerStore = useDrawerStore()
const { t } = useI18n({ useScope: 'global' });
const router = useRouter();

const busy: Ref<boolean> = ref(false);
const items: Ref<Role[]> = ref([]);
const search: Ref<string> = ref('')
const expanded: Ref<string[]> = ref([])
const selected: Ref<string[]> = ref([])
const permissionStatuses = usePermissionStatuses();

const headers = useSecureTableHeaders([
  {
    value: "action",
    headerProps: {
      class: "w-1",
    },
    permission: Permissions.Roles.Manage,
    filterable: false
  },
  {
    title: t("role-name"),
    value: "name",
  },
]);

const permissionHeaders = useSecureTableHeaders([
  {
    headerProps: {
      class: "w-96",
    },
    title: t("permission"),
    value: "name"
  },
  {
    headerProps: {
      class: "w-96",
    },
    title: t("description"),
    value: "description"
  },
  {
    title: t("status"),
    key: "status",
    value: (item: Permission) => {
      return permissionStatuses.find((status) => status.value === item.status)?.text || item.status;
    }
  },
]);

const refresh = async () => {
  busy.value = true;

  try {
    const response = await api.post("v1/roles/search", {
      shouldIncludePermissions: true,
    });
    items.value = response.data;
  } finally {
    busy.value = false;
  }
}

const remove = async (item: Role) => {
  confirmationStore.close();

  busy.value = true;

  try {
    await api.delete(`v1/roles/${item.id}`)

    useSnackbarStore().requestSent();

    refresh();
  } finally {
    busy.value = false;
  }
}

const add = () => {
  router.push({ name: "role" })
}

const json = () => {
  router.push({ name: "role-json" })
}

const upload = () => {
  router.push({ name: "role-upload" })
}

const permissions = (item: Role) => {
  router.push({ name: "role-permissions", params: { id: item.id } });
}

const rename = (item: Role) => {
  router.push({ name: "role-rename", params: { id: item.id } });
}

const download = async () => {
  if (selected.value.length === 0) {
    return;
  }

  const response = await api.post("v1/roles/bulk-download", selected.value, { responseType: 'blob' });

  const blob = new Blob([response.data], { type: 'application/json' });
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = 'roles.json';
  document.body.appendChild(a);
  a.click();
  a.remove();
}

onMounted(() => {
  refresh();

  drawerStore.initialize({
    refresh: refresh,
    parentPath: '/roles',
  })
})
</script>
