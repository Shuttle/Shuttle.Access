<template>
    <v-card flat>
        <v-card-title class="d-flex align-center pe-2">
            <div class="sv-title">{{ $t("permissions") }}</div>
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
            <template v-slot:item.rename="{ item }">
                <v-btn :icon="mdiPencil" size="x-small" @click="rename(item)" v-tooltip:end="$t('rename')" />
            </template>
            <template v-slot:item.status="{ item }">
                <div class="flex flex-row items-center my-2">
                    <v-btn-toggle v-model="item.status" variant="outlined" divided>
                        <v-btn v-for="status in statuses" v-bind:key="status.value" size="x-small" density="comfortable"
                            :disabled="item.working" :value="status.value" @click.prevent="setStatus(item)">{{ status.text }}</v-btn>
                    </v-btn-toggle>
                    <div v-if="item.working" class="flex flex-row items-center justify-center ml-2">
                        <v-icon :icon="mdiTimerSand"></v-icon>
                    </div>
                </div>
            </template>
        </v-data-table>
    </v-card>
</template>

<script setup>
import api from "@/api";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { mdiMagnify, mdiTimerSand, mdiPlus, mdiRefresh, mdiPencil } from '@mdi/js';
import { useRouter } from "vue-router";
import { useSecureTableHeaders } from "@/composables/useSecureTableHeaders";
import Permissions from "@/permissions";

const { t } = useI18n({ useScope: 'global' });
const router = useRouter();
const busy = ref(false);
const search = ref('')

const statuses = [
    {
        text: t("active"),
        value: 1
    },
    {
        text: t("deactivated"),
        value: 2
    },
    {
        text: t("removed"),
        value: 3
    },
];

const headers = useSecureTableHeaders([
    {
        value: "rename",
        headerProps: {
            class: "xl:w-1"
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

const items = ref([]);

const workingItems = computed(() => {
    return items.value.filter((item) => {
        return item.working;
    });
});

const workingCount = computed(() => {
    return workingItems.value.length;
});

const getPermissionItem = (id) => {
    return items.value.find(item => {
        return item.id === id;
    })
};

const getWorkingPermissions = () => {
    if (workingCount.value === 0) {
        return;
    }

    api
        .post(`v1/permissions/search`, {
            ids: workingItems.value.map(item => item.id)
        })
        .then(function (response) {
            response.data.forEach(item => {
                var permission = getPermissionItem(item.id);

                permission.working = permission.status !== item.status;
            });
        })
        .then(() => {
            setTimeout(() => {
                getWorkingPermissions();
            }, 1000);
        });
};

const refresh = () => {
    busy.value = true;

    api
        .get("v1/permissions")
        .then(function (response) {
            items.value = response?.data;
        })
        .finally(function () {
            busy.value = false;
        });
}

const setStatus = (permission) => {
    permission.working = true;

    api
        .patch(`v1/permissions/${permission.id}`, {
            status: permission.status
        })
        .then(function () {
            getWorkingPermissions();
        });
}

const add = () => {
    router.push({ name: "permission" })
}

const rename = (item) => {
    router.push({ name: "permission-rename", params: { id: item.id } });
}

onMounted(() => {
    refresh();
})
</script>