<template>
  <v-card flat>
    <v-card-title class="sv-card-title">
      <sv-title :title="$t('sessions')" />
      <div class="sv-strip">
        <v-btn :icon="mdiRefresh" size="x-small" @click="refresh"></v-btn>
        <v-text-field v-model="search" density="compact" :label="$t('search')" :prepend-inner-icon="mdiMagnify"
          variant="solo-filled" flat hide-details single-line></v-text-field>
      </div>
    </v-card-title>
    <v-divider></v-divider>
    <v-data-table :items="items" :headers="headers" :mobile="null" mobile-breakpoint="md" v-model:search="search"
      :loading="busy" show-expand v-model:expanded="expanded" item-value="identityName" expand-on-click>
      <template v-slot:header.action="">
        <v-btn v-if="sessionStore.hasPermission(Permissions.Sessions.Manage)" :icon="mdiDeleteSweepOutline"
          size="x-small" @click="confirmRemoveAll"></v-btn>
      </template>
      <template v-slot:item.action="{ item }">
        <v-btn :icon="mdiDeleteOutline" size="x-small"
          @click.stop="confirmationStore.show({ item: item, onConfirm: remove })" />
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
</template>

<script setup lang="ts">
import api from "@/api";
import { onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { mdiDeleteOutline, mdiDeleteSweepOutline, mdiMagnify, mdiRefresh } from '@mdi/js';
import { useConfirmationStore } from "@/stores/confirmation";
import { useSecureTableHeaders } from "@/composables/SecureTableHeaders";
import Permissions from "@/permissions";
import type { Session } from "@/access";
import type { AxiosResponse } from "axios";
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
    title: t("permission"),
    key: "permission",
    value: (item: string) => {
      return item;
    },
  },
]);

const items: Ref<Session[]> = ref([]);

const refresh = () => {
  busy.value = true;

  api
    .post("v1/sessions/search", {
      shouldIncludePermissions: true,
    })
    .then(function (response: AxiosResponse<Session[]>) {
      if (!response || !response.data) {
        return;
      }

      items.value = response.data;
    })
    .finally(function () {
      busy.value = false;
    });
}

const remove = (item: Session) => {
  confirmationStore.close();

  busy.value = true;

  api
    .delete(`v1/sessions/${item.identityId}`)
    .then(function () {
      refresh();
    })
    .finally(() => {
      busy.value = false;
    });
}

const confirmRemoveAll = () => {
  confirmationStore.show({ onConfirm: removeAll });
}

const removeAll = () => {
  confirmationStore.close();

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
