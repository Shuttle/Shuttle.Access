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
            :loading="busy" show-expand v-model:expanded="expanded" item-value="identityName" expand-on-click>
            <template v-slot:header.action="">
                <v-btn v-if="sessionStore.hasPermission(Permissions.Sessions.Manage)" :icon="mdiDeleteSweepOutline"
                    size="x-small" @click="confirmRemoveAll"></v-btn>
            </template>
            <template v-slot:item.action="{ item }">
                <v-btn :icon="mdiDelete" size="x-small"
                    @click.stop="confirmationStore.show({ item: item, onConfirm: remove })" />
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
</template>

<script setup lang="ts">
import api from "@/api";
import { onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { mdiDelete, mdiDeleteSweepOutline, mdiMagnify, mdiRefresh } from '@mdi/js';
import { useConfirmationStore } from "@/stores/confirmation";
import { useSecureTableHeaders } from "@/composables/SecureTableHeaders";
import Permissions from "@/permissions";
import type { SessionData } from "@/access";
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
            class: "w-96",
        },
        title: t("permission"),
        value: "name",
    },
    {
        headerProps: {
            class: "w-96",
        },
        title: t("description"),
        value: "description",
    }
]);

const items: Ref<SessionData[]> = ref([]);

const refresh = () => {
    busy.value = true;

    api
        .post("v1/sessions/search/data", {
            shouldIncludePermissions: true,
        })
        .then(function (response: AxiosResponse<SessionData[]>) {
            if (!response || !response.data) {
                return;
            }

            items.value = response.data;
        })
        .finally(function () {
            busy.value = false;
        });
}

const remove = (item: SessionData) => {
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
