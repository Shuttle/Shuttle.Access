<template>
  <v-card flat>
    <v-card-title class="sv-card-title">
      <a-title :title="$t('sessions')" />
      <div class="sv-strip">
        <v-btn :icon="mdiRefresh" size="x-small" @click="refresh"></v-btn>
        <v-text-field v-model="search" density="compact" :label="$t('search')" :prepend-inner-icon="mdiMagnify"
          variant="solo-filled" flat hide-details single-line></v-text-field>
      </div>
    </v-card-title>
    <v-divider></v-divider>
    <a-data-table :items="items" :headers="headers" :mobile="null" mobile-breakpoint="md" v-model:search="search"
      :loading="busy" show-expand v-model:expanded="expanded" item-value="id" expand-on-click>
      <template v-slot:header.action="">
        <v-btn v-if="sessionStore.hasPermission(Permissions.Sessions.Manage)" :icon="mdiDeleteSweepOutline"
          size="x-small" @click="removeAll()"></v-btn>
      </template>
      <template v-slot:item.action="{ item }">
        <v-btn :icon="mdiDelete" size="x-small" @click.stop="remove(item)" />
      </template>
      <template #expanded-row="{ columns, item }">
        <tr>
          <td :colspan="columns.length">
            <a-container show-border>
              <v-tabs v-model="item.tab" class="mb-2">
                <v-tab value="permissions">
                  <span>{{ $t('permissions') }}</span>
                </v-tab>
                <v-tab value="tokens">
                  <span>{{ $t('tokens') }}</span>
                </v-tab>
              </v-tabs>
              <v-tabs-window v-model="item.tab">
                <v-tabs-window-item value="permissions">
                  <a-data-table :items="item.permissions" :headers="permissionHeaders" :mobile="null"
                    mobile-breakpoint="md">
                  </a-data-table>
                </v-tabs-window-item>
                <v-tabs-window-item value="tokens">
                  <a-data-table :items="item.tokens" :headers="tokenHeaders" :mobile="null" mobile-breakpoint="md">
                  </a-data-table>
                </v-tabs-window-item>
              </v-tabs-window>
            </a-container>
          </td>
        </tr>
      </template>
    </a-data-table>
  </v-card>
</template>

<script setup lang="ts">
import api from "@/api";
import { onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { mdiDelete, mdiDeleteSweepOutline, mdiMagnify, mdiRefresh } from '@mdi/js';
import { useConfirmationStore } from "@/stores/confirmation";
import { useSecureTableHeaders } from "@/composables/SecureTableHeaders";
import Permissions from "@/permissions";
import type { Session, SessionPermission } from "@/access";
import { useDateFormatter } from "@/composables/DateFormatter";
import { useSessionStore } from "@/stores/session";

const confirmationStore = useConfirmationStore();
const sessionStore = useSessionStore();

const { t } = useI18n({ useScope: 'global' });
const busy: Ref<boolean> = ref(false);
const search: Ref<string> = ref('')
const expanded: Ref<string[]> = ref([])

const headers = useSecureTableHeaders([
  {
    value: "action",
    headerProps: {
      class: "w-1",
    },
    permission: Permissions.Sessions.Manage,
    filterable: false
  },
  {
    title: t("identity"),
    value: "identityName",
  },
  {
    title: t("description"),
    value: "identityDescription",
  },
  {
    title: t("date-registered"),
    key: "item.dateRegistered",
    value: (item: any) => {
      return useDateFormatter(item.dateRegistered).dateTimeMilliseconds();
    }
  },
  {
    title: t("expiry-date"),
    key: "item.expiryDate",
    value: (item: any) => {
      return useDateFormatter(item.expiryDate).dateTimeMilliseconds();
    }
  }
]);

const permissionHeaders = useSecureTableHeaders([
  {
    headerProps: {
      class: "w-px",
    },
    cellProps: {
      class: "whitespace-nowrap"
    },
    title: t("tenant"),
    key: "tenantName",
    value: (item: SessionPermission) => {
      return sessionStore.getTenantName(item.tenantId)
    },
  },
  {
    headerProps: {
      class: "w-fit",
    },
    title: t("permission"),
    value: "name",
  }
]);

const tokenHeaders = useSecureTableHeaders([
  {
    headerProps: {
      class: "w-px",
    },
    cellProps: {
      class: "whitespace-nowrap"
    },
    title: t("application"),
    value: "application"
  },
  {
    title: t("date-registered"),
    key: "item.dateRegistered",
    value: (item: any) => {
      return useDateFormatter(item.dateRegistered).dateTimeMilliseconds();
    }
  },
  {
    title: t("expiry-date"),
    key: "item.expiryDate",
    value: (item: any) => {
      return useDateFormatter(item.expiryDate).dateTimeMilliseconds();
    }
  },
  {
    title: t("token-hash"),
    value: "tokenHash"
  }
]);

const items: Ref<Session[]> = ref([]);

const getSelectedTab = (id: string) => {
  return items.value.find((item) => item.id === id)?.tab || 'permissions'
}

const refresh = async () => {
  busy.value = true;

  try {
    const { data } = await api.post<Session[]>("v1/sessions/search", {})

    if (!data) {
      return;
    }

    data.forEach((item) => {
      item.tab = getSelectedTab(item.id ?? '')
    })

    items.value = data;
  } finally {
    busy.value = false;
  }
}

const remove = async (item: Session) => {
  if (!(await confirmationStore.show({ messageKey: '_confirmation.remove' })).confirmed) {
    return;
  }

  busy.value = true;

  api
    .delete(`v1/sessions/${item.id}`)
    .then(function () {
      refresh();
    })
    .finally(() => {
      busy.value = false;
    });
}

const removeAll = async () => {
  if (!(await confirmationStore.show({ messageKey: '_confirmation.remove' })).confirmed) {
    return;
  }

  busy.value = true;

  api
    .delete(`v1/sessions`)
    .then(function () {
      refresh();
    })
    .finally(() => {
      busy.value = false;
    });
}

onMounted(() => {
  refresh();
})
</script>
