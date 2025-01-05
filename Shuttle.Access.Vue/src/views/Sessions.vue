<template>
  <v-card flat>
    <v-card-title class="sv-card-title">
      <div class="sv-title">{{ $t("sessions") }}</div>
      <div class="sv-strip">
        <v-btn :icon="mdiRefresh" size="small" @click="refresh"></v-btn>
        <v-btn v-if="sessionStore.hasPermission(Permissions.Sessions.Manage)" :icon="mdiAccountMultipleMinusOutline"
          size="small" @click="confirmRemoveAll" v-tooltip:end="$t('remove-all')"></v-btn>
        <v-text-field v-model="search" density="compact" :label="$t('search')" :prepend-inner-icon="mdiMagnify"
          variant="solo-filled" flat hide-details single-line></v-text-field>
      </div>
    </v-card-title>
    <v-divider></v-divider>
    <v-data-table :items="items" :headers="headers" :mobile="null" mobile-breakpoint="md" v-model:search="search"
      :loading="busy" show-expand v-model:expanded="expanded" item-value="identityName">
      <template v-slot:item.remove="{ item }">
        <v-btn :icon="mdiDeleteOutline" size="x-small"
          @click="confirmationStore.show({ item: item, onConfirm: remove })" v-tooltip:end="$t('remove')" />
      </template>
      <template #expanded-row="{ columns, item }">
        <tr>
          <td :colspan="columns.length">
            <v-list lines="one" density="compact" class="ml-4 md:ml-16">
              <v-list-subheader>{{ $t("permissions") }}</v-list-subheader>
              <v-list-item v-if="item.permissions.length" v-for="permission in item.permissions" :key="permission"
                :title="permission">
                <template v-slot:prepend>
                  <v-icon :icon="mdiShieldOutline" size="x-small"></v-icon>
                </template>
              </v-list-item>
              <v-list-item v-else :title="t('none')">
                <template v-slot:prepend>
                  <v-icon :icon="mdiShieldOffOutline" size="x-small"></v-icon>
                </template>
              </v-list-item>
            </v-list>
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
import { mdiAccountMultipleMinusOutline, mdiDeleteOutline, mdiMagnify, mdiRefresh, mdiShieldOffOutline, mdiShieldOutline } from '@mdi/js';
import { useRouter } from "vue-router";
import { useConfirmationStore } from "@/stores/confirmation";
import { useSecureTableHeaders } from "@/composables/useSecureTableHeaders";
import Permissions from "@/permissions";
import type { Session } from "@/access";
import type { AxiosResponse } from "axios";
import { useDateTimeFormatter } from "@/composables/useDateFormatter";
import { useSessionStore } from "@/stores/session";

const confirmationStore = useConfirmationStore();
const sessionStore = useSessionStore();

const { t } = useI18n({ useScope: 'global' });
const router = useRouter();
const busy: Ref<boolean> = ref(false);
const search: Ref<string> = ref('')
const expanded: Ref<string[]> = ref([])

const headers = useSecureTableHeaders([
  {
    value: "remove",
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
      return useDateTimeFormatter(item.dateActivated);
    }
  },
  {
    title: t("expiry-date"),
    key: "item.expiryDate",
    value: (item: any) => {
      return useDateTimeFormatter(item.expiryDate);
    }
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
    .delete(`v1/sessions/${item.token}`)
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
