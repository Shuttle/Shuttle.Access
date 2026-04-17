<template>
  <div>
    <div class="flex flex-col sm:flex-row items-stretch gap-2 p-2">
      <div v-for="item in items" :key="item.route"
        class="group flex flex-col items-center justify-center border border-solid rounded-lg p-2 w-full cursor-pointer hover:bg-[color:rgb(var(--v-theme-primary--hover))] active:bg-[color:rgb(var(--v-theme-primary--active))]"
        @click="click(item)">
        <v-icon :icon="item.svg"
          class="text-[color:rgb(var(--v-theme-primary--hover))] group-hover:text-[color:rgb(var(--v-theme-secondary--hover))]"></v-icon>
        <div class="text-2xl font-bold group-hover:text-[color:rgb(var(--v-theme-primary-text--hover))]">
          {{ item.title }}</div>
        <div class="text-xl font-semibold">{{ item.value }}</div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { DashboardItem } from "@/access";
import api from "@/api";
import { mdiAccount, mdiAccountGroup, mdiBadgeAccount, mdiDomain, mdiShield } from "@mdi/js";
import { onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";

const { t } = useI18n({ useScope: 'global' });
const router = useRouter();

const items = ref<DashboardItem[]>([]);

const click = (item: DashboardItem) => {
  router.push({ name: item.route })
}

const addItem = (title: string, value: number, route: string, svg: string) => {
  items.value.push({ title: title, value: value, route: route, svg: svg });
}

const refresh = async () => {
  const response = await api.get("v1/statistics/dashboard");

  addItem(t("permissions"), response.data.permissionCount, "permissions", mdiShield);
  addItem(t("identities"), response.data.identityCount, "identities", mdiAccount);
  addItem(t("roles"), response.data.roleCount, "roles", mdiAccountGroup);
  addItem(t("sessions"), response.data.sessionCount, "sessions", mdiBadgeAccount);
  addItem(t("tenants"), response.data.tenantCount, "tenants", mdiDomain);
}

onMounted(() => {
  refresh();
})
</script>
