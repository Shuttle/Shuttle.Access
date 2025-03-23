<template>
  <v-card flat>
    <v-card-title class="sv-card-title">
      <sv-title :title="`${t('permissions')} - ${name}`" close-path="/roles" type="borderless" />
      <div class="sv-strip">
        <v-btn :icon="mdiRefresh" size="small" @click="refresh"></v-btn>
        <v-text-field v-model="search" density="compact" :label="$t('search')" :prepend-inner-icon="mdiMagnify"
          variant="solo-filled" flat hide-details single-line></v-text-field>
      </div>
    </v-card-title>
    <v-divider></v-divider>
    <v-data-table :items="items" :headers="headers" :mobile="null" mobile-breakpoint="md" v-model:search="search"
      :loading="busy">
      <template v-slot:item.active="{ item }">
        <v-icon v-if="item.working" :icon="mdiTimerSand"></v-icon>
        <v-checkbox-btn v-else v-model="item.active" @update:model-value="toggle(item)" />
      </template>
      <template v-slot:item.statusIcon="{ item }">
        <v-icon :icon="item.statusIcon"></v-icon>
      </template>
      <template v-slot:item.permission="{ item }">
        <span :class="!item.active ? 'text-gray-500' : ''">{{ item.name }}</span>
      </template>
    </v-data-table>
  </v-card>
</template>

<script setup lang="ts">
import api from "@/api";
import { computed, onMounted, reactive, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useAlertStore } from "@/stores/alert";
import { mdiMagnify, mdiTimerSand, mdiPlayCircleOutline, mdiStopCircleOutline, mdiCloseCircleOutline, mdiRefresh } from '@mdi/js';
import type { IdentifierAvailability, Permission } from "@/access";

const { t } = useI18n({ useScope: 'global' });
const alertStore = useAlertStore();

const props = defineProps<{
  id: string;
}>();

type PermissionItem = Permission & {
  active: boolean;
  activeOnToggle: boolean;
  working: boolean;
  statusName: string;
  statusIcon: string;
}

const name: Ref<string> = ref('');
const rolePermissions: Ref<PermissionItem[]> = ref([]);
const permissions = ref([]);
const busy: Ref<boolean> = ref(false);
const search: Ref<string> = ref('');

const headers: any = [
  {
    value: "active",
    headerProps: {
      class: "w-1"
    },
    align: "center",
    filterable: false
  },
  {
    value: "statusIcon",
    headerProps: {
      class: "w-1"
    },
    filterable: false
  },
  {
    title: t("permission"),
    value: "name",
  },
  {
    title: t("status"),
    value: "statusName"
  }
];

const items: ComputedRef<PermissionItem[]> = computed(() => {
  const result: PermissionItem[] = [];

  rolePermissions.value
    .forEach(item => {
      result.push(reactive({
        ...item,
        active: true,
        working: false,
      }));
    });

  permissions.value
    .filter((item: PermissionItem) => {
      return !result.some((r) => r.id == item.id);
    })
    .forEach((item: PermissionItem) => {
      result.push(reactive({
        ...item,
        active: false,
        working: false,
      }));
    });

  const active = t("active");
  const deactivated = t("deactivated");
  const removed = t("removed");

  result.forEach((item: PermissionItem) => {
    let statusName = active;
    let statusIcon = mdiPlayCircleOutline;

    switch (item.status) {
      case 2: {
        statusName = deactivated;
        statusIcon = mdiStopCircleOutline;
        break;
      }
      case 3: {
        statusName = removed;
        statusIcon = mdiCloseCircleOutline;
        break;
      }
    }

    item.statusName = statusName;
    item.statusIcon = statusIcon;
  });

  return result;
});

const refresh = async () => {
  const roleResponse = await api.get(`v1/roles/${props.id}`)

  name.value = roleResponse.data.name;
  rolePermissions.value = roleResponse.data.permissions;

  const permissionsResponse = await api.post("v1/permissions/search", {})

  permissions.value = permissionsResponse.data;
};

const workingItems: ComputedRef<PermissionItem[]> = computed(() => {
  return items.value.filter((item) => {
    return item.working;
  });
});

const workingCount: ComputedRef<number> = computed(() => {
  return workingItems.value.length;
});

const getPermissionItem = (id: string) => {
  return items.value.find(item => item.id === id);
};

const getPermissionAvailability = async () => {
  if (workingCount.value === 0) {
    return;
  }

  const response = await api.post<IdentifierAvailability[]>(`v1/roles/${props.id}/permissions/availability`, {
    values: workingItems.value.map(item => item.id)
  })

  response.data.forEach(availability => {
    const permissionItem = getPermissionItem(availability.id);

    if (!permissionItem) {
      return;
    }

    permissionItem.working = permissionItem.activeOnToggle ? availability.active : !availability.active;
  });

  setTimeout(() => {
    getPermissionAvailability();
  }, 1000);
};

const toggle = async (item: PermissionItem) => {
  if (item.working) {
    alertStore.working();
    return;
  }

  item.working = true;
  item.activeOnToggle = !item.active;

  await api.patch(`v1/roles/${props.id}/permissions`, {
    permissionId: item.id,
    active: item.active,
  });

  getPermissionAvailability();
}

onMounted(() => {
  refresh();
})
</script>
