<template>
    <v-card flat>
        <v-card-title class="d-flex align-center pe-2">
            <div class="sv-title">{{ `${t("roles")} - ${name}` }}</div>
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
        </v-data-table>
    </v-card>
</template>

<script setup>
import api from "@/api";
import { computed, onMounted, reactive, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useRoute } from 'vue-router';
import { useAlertStore } from "@/stores/alert";
import { mdiTimerSand, mdiMagnify, mdiRefresh } from '@mdi/js';

const { t } = useI18n({ useScope: 'global' });
const alertStore = useAlertStore();

const id = ref(useRoute().params.id);
const name = ref('');
const identityRoles = ref([]);
const roles = ref([]);
const busy = ref(false);
const search = ref('');

const headers = [
    {
        value: "active",
        headerProps: {
            class: "w-1"
        },
    },
    {
        title: t("role-name"),
        value: "roleName",
    }
];

const items = computed(() => {
    var result = [];

    identityRoles.value.forEach(item => {
        result.push(reactive({
            roleId: item.id,
            roleName: item.name,
            active: true,
            working: false,
        }));
    });
    roles.value.filter((item) => {
        return !result.some((r) => r.roleId == item.id);
    }).forEach(item => {
        result.push(reactive({
            roleId: item.id,
            roleName: item.name,
            active: false,
            working: false,
        }));
    });

    return result;
});

const refresh = () => {
    api.get(`v1/identities/${id.value}`).then((response) => {
        name.value = response.data.name;
        identityRoles.value = response.data.roles;

        api.get("v1/roles").then((response) => {
            roles.value = response.data;
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

const getRoleItem = (id) => {
    return items.value.find(item => item.roleId === id);
};

const getWorkingRoles = () => {
    if (workingCount.value === 0) {
        return;
    }

    api
        .post(`v1/identities/${id.value}/roles/availability`, {
            values: workingItems.value.map(item => item.roleId)
        })
        .then(function (response) {
            response.data.forEach(availability => {
                getRoleItem(availability.id).working = availability.active;
            });
        })
        .then(() => {
            setTimeout(() => {
                getWorkingRoles();
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
        .patch(`v1/identities/${id.value}/roles/${item.roleId}`, {
            active: item.active,
        });

    getWorkingRoles();
}

onMounted(() => {
    refresh();
})
</script>