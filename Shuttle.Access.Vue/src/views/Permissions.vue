<template>
  <v-card flat>
    <v-card-title class="sv-card-title">
      <sv-title :title="$t('permissions')" />
      <div class="sv-strip">
        <v-btn :icon="mdiRefresh" size="x-small" @click="refresh"></v-btn>
        <v-text-field v-model="search" density="compact" :label="$t('search')" :prepend-inner-icon="mdiMagnify"
          variant="solo-filled" flat hide-details single-line></v-text-field>
      </div>
    </v-card-title>
    <v-divider></v-divider>
    <v-data-table :items="items" :headers="headers" :mobile="null" mobile-breakpoint="md" v-model:search="search"
      :loading="busy" show-select v-model="selected">
      <template v-slot:header.action="">
        <div class="flex flex-row items-center gap-2" v-if="sessionStore.hasPermission(Permissions.Roles.Manage)">
          <v-btn :icon="mdiPlus" size="x-small" @click="add"></v-btn>
          <v-btn :icon="mdiCodeJson" size="x-small" @click="json"></v-btn>
          <v-btn :icon="mdiDownload" size="x-small" @click="download" v-if="selected.length"></v-btn>
        </div>
      </template>
      <template v-slot:item.action="{ item }">
        <v-btn :icon="mdiPencil" size="x-small" @click="rename(item)" />
      </template>
      <template v-slot:item.status="{ item }">
        <div class="flex flex-row items-center my-2">
          <v-btn-toggle v-model="item.status" variant="outlined" divided>
            <v-btn v-for="status in statuses" v-bind:key="status.value" size="x-small" density="compact"
              :disabled="item.working" :value="status.value" @click.prevent="setStatus(item)">{{
                status.text }}</v-btn>
          </v-btn-toggle>
          <div v-if="item.working" class="flex flex-row items-center justify-center ml-2">
            <v-icon :icon="mdiTimerSand"></v-icon>
          </div>
        </div>
      </template>
    </v-data-table>
  </v-card>
  <sv-form-drawer></sv-form-drawer>
</template>

<script setup lang="ts">
import api from "@/api";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { mdiCodeJson, mdiMagnify, mdiTimerSand, mdiPlus, mdiRefresh, mdiPencil, mdiDownload } from '@mdi/js';
import { useRouter } from "vue-router";
import { useSecureTableHeaders } from "@/composables/SecureTableHeaders";
import Permissions from "@/permissions";
import type { Permission } from "@/access";
import { useSessionStore } from "@/stores/session";
import { usePermissionStatuses } from "@/composables/Data";
import { useDrawerStore } from "@/stores/drawer";

const sessionStore = useSessionStore();
const { t } = useI18n({ useScope: 'global' });
const router = useRouter();
const drawerStore = useDrawerStore()

const busy: Ref<boolean> = ref(false);
const search: Ref<string> = ref('')
const selected: Ref<string[]> = ref([])

type PermissionItem = Permission & {
  working: boolean;
}

const statuses = usePermissionStatuses();

const headers = useSecureTableHeaders([
  {
    value: "action",
    headerProps: {
      class: "w-1"
    },
    permission: Permissions.Permissions.Manage,
    filterable: false
  },
  {
    title: t("status"),
    value: "status",
    headerProps: {
      class: "xl:w-72"
    },
    permission: Permissions.Permissions.Manage,
    filterable: false
  },
  {
    title: t("permission"),
    value: "name",
  },
]);

const items: Ref<PermissionItem[]> = ref([]);

const workingItems = computed(() => {
  return items.value.filter((item: any) => {
    return item.working;
  });
});

const workingCount = computed(() => {
  return workingItems.value.length;
});

const getPermissionItem = (id: string): PermissionItem | undefined => {
  return items.value.find((item: PermissionItem) => {
    return item.id === id;
  })
};

const getWorkingPermissions = async () => {
  if (workingCount.value === 0) {
    return;
  }

  try {
    const response = await api.post(`v1/permissions/search`, {
      ids: workingItems.value.map(item => item.id)
    });

    response.data.forEach((item: PermissionItem) => {
      const permission = getPermissionItem(item.id);

      if (!permission) {
        return;
      }

      permission.working = permission.status !== item.status;
    });
  } finally {
    setTimeout(() => {
      getWorkingPermissions();
    }, 1000);
  }
};

const refresh = async () => {
  busy.value = true;

  try {
    const response = await api.post("v1/permissions/search", {});

    items.value = response?.data;
  } finally {
    busy.value = false;
  }
}

const setStatus = async (permission: PermissionItem) => {
  permission.working = true;

  try {
    await api.patch(`v1/permissions/${permission.id}`, {
      status: permission.status
    });

    getWorkingPermissions();
  } finally {
    permission.working = false;
  }
}

const add = () => {
  router.push({ name: "permission" })
}

const json = () => {
  router.push({ name: "permission-json" })
}

const rename = (item: PermissionItem) => {
  router.push({ name: "permission-rename", params: { id: item.id } });
}

const download = async () => {
  if (selected.value.length === 0) {
    return;
  }

  const response = await api.post("v1/permissions/bulk-download", selected.value, { responseType: 'blob' });

  const blob = new Blob([response.data], { type: 'application/json' });
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = 'permissions.json';
  document.body.appendChild(a);
  a.click();
  a.remove();
}

onMounted(() => {
  refresh();

  drawerStore.initialize({
    refresh: refresh,
    parentPath: '/permissions',
  })
})
</script>
