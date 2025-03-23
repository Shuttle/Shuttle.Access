<template>
  <v-card flat>
    <v-card-title class="sv-card-title">
      <sv-title :title="$t('roles')" />
      <div class="sv-strip">
        <v-btn :icon="mdiRefresh" size="small" @click="refresh"></v-btn>
        <v-btn v-if="sessionStore.hasPermission(Permissions.Roles.Manage)" :icon="mdiPlus" size="small"
          @click="add"></v-btn>
        <v-text-field v-model="search" density="compact" :label="$t('search')" :prepend-inner-icon="mdiMagnify"
          variant="solo-filled" flat hide-details single-line></v-text-field>
      </div>
    </v-card-title>
    <v-divider></v-divider>
    <v-data-table :items="items" :headers="headers" :mobile="null" mobile-breakpoint="md" v-model:search="search"
      :loading="busy" show-expand v-model:expanded="expanded" item-value="name" expand-on-click>
      <template v-slot:item.permissions="{ item }">
        <v-btn :icon="mdiShieldOutline" size="x-small" @click.stop="permissions(item)"
          v-tooltip:end="$t('permissions')" />
      </template>
      <template v-slot:item.rename="{ item }">
        <v-btn :icon="mdiPencil" size="x-small" @click.stop="rename(item)" v-tooltip:end="$t('rename')" />
      </template>
      <template v-slot:item.remove="{ item }">
        <v-btn :icon="mdiDeleteOutline" size="x-small"
          @click.stop="confirmationStore.show({ item: item, onConfirm: remove })" v-tooltip:end="$t('remove')" />
      </template>
      <template #expanded-row="{ columns, item }">
        <tr>
          <td :colspan="columns.length">
            <div class="sv-table-container">
              <v-data-table :items="item.permissions" :headers="permissionHeaders" :mobile="null"
                mobile-breakpoint="md">
              </v-data-table>
            </div>
          </td>
        </tr>
      </template>
    </v-data-table>
  </v-card>
  <sv-form-drawer v-if="drawer" close-path="/roles">
  </sv-form-drawer>
</template>

<script setup lang="ts">
import api from "@/api";
import { onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { mdiDeleteOutline, mdiMagnify, mdiPlus, mdiRefresh, mdiPencil, mdiShieldOutline } from '@mdi/js';
import { useRouter } from "vue-router";
import { useAlertStore } from "@/stores/alert";
import { useConfirmationStore } from "@/stores/confirmation";
import { useSecureTableHeaders } from "@/composables/SecureTableHeaders";
import Permissions from "@/permissions";
import type { Permission, Role } from "@/access";
import { useSessionStore } from "@/stores/session";
import { usePermissionStatuses } from "@/composables/Data";

const confirmationStore = useConfirmationStore();
const sessionStore = useSessionStore();

const { t } = useI18n({ useScope: 'global' });
const router = useRouter();
const route = useRoute()

const busy: Ref<boolean> = ref(false);
const search: Ref<string> = ref('')
const expanded: Ref<string[]> = ref([])
const permissionStatuses = usePermissionStatuses();
const drawer = ref(false)

const headers = useSecureTableHeaders([
  {
    value: "permissions",
    headerProps: {
      class: "w-1",
    },
    permission: Permissions.Roles.Manage,
    filterable: false
  },
  {
    value: "rename",
    headerProps: {
      class: "w-1",
    },
    permission: Permissions.Roles.Manage,
    filterable: false
  },
  {
    value: "remove",
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
    title: t("permission"),
    value: "name"
  },
  {
    title: t("status"),
    key: "status",
    value: (item: Permission) => {
      return permissionStatuses.find((status) => status.value === item.status)?.text || item.status;
    }
  },
]);

const items: Ref<Role[]> = ref([]);

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
    useAlertStore().requestSent();

    refresh();
  } finally {
    busy.value = false;
  }
}

const add = () => {
  router.push({ name: "role" })
}

const permissions = (item: Role) => {
  router.push({ name: "role-permissions", params: { id: item.id } });
}

const rename = (item: Role) => {
  router.push({ name: "role-rename", params: { id: item.id } });
}

watch(
  () => route.fullPath,
  async (fullPath) => {
    drawer.value = !fullPath.endsWith('/roles')

    if (!drawer.value) {
      await refresh()
    }
  },
)

onMounted(() => {
  refresh();
})
</script>
