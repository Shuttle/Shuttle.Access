<template>
    <v-card flat>
        <v-card-title class="d-flex align-center pe-2">
            <div class="sv-title">{{ $t("roles") }}</div>
            <div class="sv-strip">
                <v-btn :icon="mdiRefresh" size="small" @click="refresh"></v-btn>
                <v-btn :icon="mdiPlus" size="small" @click="add"></v-btn>
                <v-text-field v-model="search" density="compact" :label="$t('search')" :prepend-inner-icon="mdiMagnify"
                    variant="solo-filled" flat hide-details single-line></v-text-field>
            </div>
        </v-card-title>
        <v-divider></v-divider>
        <v-data-table :items="items" :headers="headers" :mobile="null" mobile-breakpoint="md" v-model:search="search" :loading="busy">
            <template v-slot:text>
                <v-text-field v-model="search" label="Search" prepend-inner-icon="mdi-magnify" variant="outlined"
                    hide-details single-line></v-text-field>
            </template>
            <template v-slot:item.permissions="{ item }">
                <v-btn :icon="mdiShieldOutline" size="x-small" @click="permissions(item)" v-tooltip:end="$t('permissions')"/>
            </template>
            <template v-slot:item.rename="{ item }">
                <v-btn :icon="mdiPencil" size="x-small" @click="rename(item)" v-tooltip:end="$t('rename')" />
            </template>
            <template v-slot:item.remove="{ item }">
                <v-btn :icon="mdiDeleteOutline" size="x-small" @click="confirmationStore.show(item, remove)" v-tooltip:end="$t('remove')"/>
            </template>
        </v-data-table>
    </v-card>
</template>

<script setup>
import api from "@/api";
import { onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { mdiDeleteOutline, mdiMagnify, mdiPlus, mdiRefresh, mdiPencil, mdiShieldOutline } from '@mdi/js';
import { useRouter } from "vue-router";
import { useAlertStore } from "@/stores/alert";
import { useConfirmationStore } from "@/stores/confirmation";
import { useSecureTableHeaders } from "@/composables/useSecureTableHeaders";
import Permissions from "@/permissions";

var confirmationStore = useConfirmationStore();

const { t } = useI18n({ useScope: 'global' });
const router = useRouter();
const busy = ref(false);
const search = ref('')

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

const items = ref([]);

const refresh = () => {
    busy.value = true;

    api
        .get("v1/roles")
        .then(function (response) {
            if (!response || !response.data) {
                return;
            }

            items.value = response.data;
        })
        .finally(function () {
            busy.value = false;
        });
}

const remove = (item) => {
    confirmationStore.setIsOpen(false);

    busy.value = true;

    api
        .delete(`v1/roles/${item.id}`)
        .then(function () {
            useAlertStore().requestSent();

            refresh();
        })
        .finally(() => {
            busy.value = false;
        });
}

const add = () => {
    router.push({ name: "role" })
}

const permissions = (item) => {
    router.push({ name: "role-permissions", params: { id: item.id } });
}

const rename = (item) => {
    router.push({ name: "role-rename", params: { id: item.id } });
}

onMounted(() => {
    refresh();
})
</script>
