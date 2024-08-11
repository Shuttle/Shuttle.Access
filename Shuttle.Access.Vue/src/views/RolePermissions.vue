<template>
    <v-card flat>
        <v-card-title class="d-flex align-center pe-2">
            <div class="sv-title">{{ `${t("permissions")} - ${name}` }}</div>
            <div class="sv-strip">
                <v-btn :icon="mdiRefresh" size="small" @click="refresh"></v-btn>
                <v-text-field v-model="search" density="compact" :label="$t('search')" :prepend-inner-icon="mdiMagnify"
                    variant="solo-filled" flat hide-details single-line></v-text-field>
            </div>
        </v-card-title>
        <v-divider></v-divider>
        <v-data-table :items="items" :headers="headers" :mobile="null" mobile-breakpoint="md" v-model:search="search"
            :loading="busy">
            <template v-slot:text>
                <v-text-field v-model="search" label="Search" prepend-inner-icon="mdi-magnify" variant="outlined"
                    hide-details single-line></v-text-field>
            </template>
            <template v-slot:item.active="{ item }">
                <v-icon v-if="item.working" :icon="mdiTimerSand"></v-icon>
                <v-checkbox-btn v-else v-model="item.active" @update:model-value="toggle(item)" />
            </template>
            <template v-slot:item.statusIcon="{ item }">
                <v-icon :icon="item.statusIcon"></v-icon>
            </template>
            <template v-slot:item.permission="{ item }">
                <span :class="!item.active ? 'text-gray-500' : ''">{{ item.permission }}</span>
            </template>
        </v-data-table>
    </v-card>
</template>

<script setup>
import api from "@/api";
import { computed, onMounted, reactive, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useRoute } from 'vue-router';
import { useAlertStore } from "@/stores/alert";
import { mdiTimerSand, mdiPlayCircleOutline, mdiStopCircleOutline, mdiCloseCircleOutline, mdiRefresh } from '@mdi/js';

const { t } = useI18n({ useScope: 'global' });
const alertStore = useAlertStore();

const id = ref(useRoute().params.id);
const name = ref('');
const rolePermissions = ref([]);
const permissions = ref([]);
const busy = ref();
const search = ref('')

const headers = [
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

const items = computed(() => {
    var result = [];

    rolePermissions.value
        .forEach(item => {
            result.push(reactive({
                ...item,
                active: true,
                working: false,
            }));
        });

    permissions.value
        .filter((item) => {
            return !result.some((r) => r.id == item.id);
        })
        .forEach(item => {
            result.push(reactive({
                ...item,
                active: false,
                working: false,
            }));
        });

    const active = t("active");
    const deactivated = t("deactivated");
    const removed = t("removed");

    result.forEach(item => {
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

const refresh = () => {
    api.get(`v1/roles/${id.value}`).then((response) => {
        name.value = response.data.name;
        rolePermissions.value = response.data.permissions;

        api.get("v1/permissions")
            .then((response) => {
                permissions.value = response.data;
            });
    });
};

const workingItems = computed(() => {
    return items.value.filter((item) => {
        return item.working;
    });
});

const workingCount = computed(() => {
    return workingItems.value.length;
});

const getPermissionItem = (id) => {
    return items.value.find(item => item.id === id);
};

const getPermissionAvailability = () => {
    if (workingCount.value === 0) {
        return;
    }

    api
        .post(`v1/roles/${id.value}/permissions/availability`, {
            values: workingItems.value.map(item => item.id)
        })
        .then(function (response) {
            response.data.forEach(availability => {
                getPermissionItem(availability.id).working = availability.active;
            });
        })
        .then(() => {
            setTimeout(() => {
                getPermissionAvailability();
            }, 1000);
        });
};

const toggle = (item) => {
    if (item.working) {
        alertStore.working();
        return;
    }

    item.working = true;

    api
        .patch(`v1/roles/${id.value}/permissions`, {
            permissionId: item.id,
            active: item.active,
        });

    getPermissionAvailability();
}

onMounted(() => {
    refresh();
})
</script>