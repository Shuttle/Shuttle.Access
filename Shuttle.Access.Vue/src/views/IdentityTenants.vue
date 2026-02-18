<template>
  <v-card flat>
    <v-card-title class="sv-card-title">
      <a-title :title="`${t('tenants')} - ${name}`" close-drawer type="borderless"></a-title>
      <div class="sv-strip">
        <v-btn :icon="mdiRefresh" size="small" @click="refresh"></v-btn>
        <v-text-field v-model="search" density="compact" :label="$t('search')" :prepend-inner-icon="mdiMagnify"
          variant="solo-filled" flat hide-details single-line></v-text-field>
      </div>
    </v-card-title>
    <v-divider></v-divider>
    <a-data-table :items="items" :headers="headers" :mobile="null" mobile-breakpoint="md" v-model:search="search"
      :loading="busy">
      <template v-slot:item.active="{ item }">
        <v-icon v-if="item.working" :icon="mdiTimerSand"></v-icon>
        <v-checkbox-btn v-else v-model="item.active" @update:model-value="toggle(item)" />
      </template>
    </a-data-table>
  </v-card>
</template>

<script setup lang="ts">
import api from "@/api";
import { computed, onMounted, reactive, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useRoute } from 'vue-router';
import { mdiTimerSand, mdiMagnify, mdiRefresh } from '@mdi/js';
import type { IdentifierAvailability } from "@/access";
import { useSnackbarStore } from "@/stores/snackbar";

const { t } = useI18n({ useScope: 'global' });
const snackbarStore = useSnackbarStore();

const id: Ref<string | string[]> = ref(useRoute().params.id);
const name: Ref<string> = ref('');
const identityTenants: Ref = ref([]);
const tenants: Ref = ref([]);
const busy: Ref<boolean> = ref(false);
const search = ref('');
let timeoutId: ReturnType<typeof setTimeout> | undefined;

const headers: any = [
  {
    value: "active",
    align: "center",
    headerProps: {
      class: "w-1"
    },
  },
  {
    title: t("name"),
    value: "tenantName",
  }
];

export type TenantItem = {
  tenantId: string;
  tenantName: string;
  active: boolean;
  activeOnToggle?: boolean;
  working: boolean;
};

const items = computed(() => {
  const result: TenantItem[] = [];

  identityTenants.value.forEach((item: any) => {
    result.push(reactive({
      tenantId: item.id,
      tenantName: item.name,
      active: true,
      working: false,
    }));
  });

  tenants.value.filter((item: any) => {
    return !result.some((r) => r.tenantId == item.id);
  }).forEach((item: any) => {
    result.push(reactive({
      tenantId: item.id,
      tenantName: item.name,
      active: false,
      working: false,
    }));
  });

  return result;
});

const refresh = async () => {
  busy.value = true;

  try {
    const tenantResponse = await api.post("v1/tenants/search", {});

    tenants.value = tenantResponse.data;

    const identityResponse = await api.get(`v1/identities/${id.value}`);

    name.value = identityResponse.data.name;
    identityTenants.value = identityResponse.data.tenants;
  } finally {
    busy.value = false;
  }
};

const workingItems = computed(() => {
  return items.value.filter((item) => {
    return item.working;
  });
});

const workingCount = computed(() => {
  return workingItems.value.length;
});

const getTenantAssignment = (id: string): TenantItem | undefined => {
  return items.value.find(item => item.tenantId === id);
};

const getWorkingTenants = async () => {
  if (workingCount.value === 0) {
    return;
  }

  const response = await api.post(`v1/identities/${id.value}/tenants/availability`, {
    values: workingItems.value.map(item => item.tenantId)
  });

  response.data.forEach((availability: IdentifierAvailability) => {
    const tenantItem = getTenantAssignment(availability.id);

    if (!tenantItem) {
      return;
    }

    tenantItem.working = tenantItem.activeOnToggle ? availability.active : !availability.active;
  });

  timeoutId = setTimeout(async () => {
    await getWorkingTenants();
  }, 1000);
};

const toggle = async (item: TenantItem) => {
  if (item.working) {
    snackbarStore.working();
    return;
  }

  item.working = true;
  item.activeOnToggle = !item.active;

  await api.patch(`v1/identities/${id.value}/tenants/${item.tenantId}`, {
    active: item.active,
  });

  getWorkingTenants();
}

onMounted(() => {
  refresh();
})

onBeforeUnmount(() => {
  if (timeoutId) {
    clearTimeout(timeoutId);
    timeoutId = undefined;
  }
});
</script>
