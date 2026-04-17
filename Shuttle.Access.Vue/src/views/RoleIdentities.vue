<template>
  <v-card flat>
    <v-card-title class="sv-card-title">
      <a-title :title="`${t('identities')} - ${role?.name}`" close-drawer type="borderless" />
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
import { mdiMagnify, mdiRefresh, mdiTimerSand } from '@mdi/js';
import type { Identity, IdentitySpecification, Role, RoleIdentity } from "@/access";
import { useSnackbarStore } from "@/stores/snackbar";
import type { AxiosResponse } from "axios";

const { t } = useI18n({ useScope: 'global' });
const snackbarStore = useSnackbarStore();

const props = defineProps<{
  id: string;
}>();

type IdentityItem = RoleIdentity & {
  active: boolean;
  activeOnToggle: boolean;
  working: boolean;
}

const role: Ref<Role | null> = ref(null);
const identities: Ref<Identity[]> = ref([]);
const busy: Ref<boolean> = ref(false);
const search: Ref<string> = ref('');
let timeoutId: ReturnType<typeof setTimeout> | undefined;

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
    title: t("name"),
    value: "name",
  },
  {
    title: t("description"),
    value: "description"
  }
];

const items: ComputedRef<IdentityItem[]> = computed(() => {
  const result: IdentityItem[] = [];

  if (!role.value) {
    return [];
  }

  identities.value
    .forEach(identity => {
      result.push(reactive({
        id: identity.id,
        name: identity.name,
        description: identity.description,
        active: (role.value!.identities ?? []).some(item => item.id == identity.id),
        activeOnToggle: false,
        working: false,
      }));
    });

  return result;
});

const refresh = async () => {
  const roleResponse = await api.get(`v1/roles/${props.id}`)

  role.value = roleResponse.data;

  const identityResponse = await api.post<IdentitySpecification, AxiosResponse<Identity[]>>("v1/identities/search", {})

  identities.value = identityResponse.data;
};

const workingItems: ComputedRef<IdentityItem[]> = computed(() => {
  return items.value.filter((item) => {
    return item.working;
  });
});

const workingCount: ComputedRef<number> = computed(() => {
  return workingItems.value.length;
});

const getIdentityItem = (id: string) => {
  return items.value.find(item => item.id === id);
};

const getIdentityAvailability = async () => {
  if (workingCount.value === 0) {
    return;
  }

  const { data } = await api.post<IdentitySpecification, AxiosResponse<Identity[]>>(`v1/identities/search`, {
    ids: workingItems.value.map(item => item.id),
    shouldIncludeRoles: true
  })

  data.forEach(identity => {
    const identityItem = getIdentityItem(identity.id);

    if (!identityItem) {
      return;
    }

    identity.roles ??= [];

    const hasRole = identity.roles.some(item => item.id == role.value!.id);

    identityItem.working = identityItem.activeOnToggle ? hasRole : !hasRole;
  });

  timeoutId = setTimeout(() => {
    getIdentityAvailability();
  }, 1000);
};

const toggle = async (item: IdentityItem) => {
  if (item.working) {
    snackbarStore.working();
    return;
  }

  item.working = true;
  item.activeOnToggle = !item.active;

  await api.patch(`v1/identities/${item.id}/roles/${props.id}/status`, {
    active: item.active,
  });

  getIdentityAvailability();
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
