<template>
    <v-card flat>
        <v-card-title class="sv-card-title">
            <a-title :title="`${t('roles')} - ${name}`" close-drawer type="borderless"></a-title>
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
const identityRoles: Ref = ref([]);
const roles: Ref = ref([]);
const busy: Ref<boolean> = ref(false);
const search = ref('');

const headers: any = [
    {
        value: "active",
        align: "center",
        headerProps: {
            class: "w-1"
        },
    },
    {
        title: t("role-name"),
        value: "roleName",
    }
];

export type RoleItem = {
    roleId: string;
    roleName: string;
    active: boolean;
    activeOnToggle?: boolean;
    working: boolean;
};

const items = computed(() => {
    const result: RoleItem[] = [];

    identityRoles.value.forEach((item: any) => {
        result.push(reactive({
            roleId: item.id,
            roleName: item.name,
            active: true,
            working: false,
        }));
    });

    roles.value.filter((item: any) => {
        return !result.some((r) => r.roleId == item.id);
    }).forEach((item: any) => {
        result.push(reactive({
            roleId: item.id,
            roleName: item.name,
            active: false,
            working: false,
        }));
    });

    return result;
});

const refresh = async () => {
    busy.value = true;

    try {
        const roleResponse = await api.post("v1/roles/search", {});

        roles.value = roleResponse.data;

        const identityResponse = await api.get(`v1/identities/${id.value}`);

        name.value = identityResponse.data.name;
        identityRoles.value = identityResponse.data.roles;
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

const getRoleAssignment = (id: string): RoleItem | undefined => {
    return items.value.find(item => item.roleId === id);
};

const getWorkingRoles = async () => {
    if (workingCount.value === 0) {
        return;
    }

    const response = await api.post(`v1/identities/${id.value}/roles/availability`, {
        values: workingItems.value.map(item => item.roleId)
    });

    response.data.forEach((availability: IdentifierAvailability) => {
        const roleItem = getRoleAssignment(availability.id);

        if (!roleItem) {
            return;
        }

        roleItem.working = roleItem.activeOnToggle ? availability.active : !availability.active;
    });

    setTimeout(async () => {
        await getWorkingRoles();
    }, 1000);
};

const toggle = (item: RoleItem) => {
    if (item.working) {
        snackbarStore.working();
        return;
    }

    item.working = true;
    item.activeOnToggle = !item.active;

    api
        .patch(`v1/identities/${id.value}/roles/${item.roleId}`, {
            active: item.active,
        });

    getWorkingRoles();
}

onMounted(() => {
    refresh();
})
</script>
