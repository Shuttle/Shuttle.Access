<template>
  <div>
    <div class="flex flex-col sm:flex-row items-stretch gap-2 p-2">
      <div v-for="item in items" :key="item.route"
        class="group flex flex-col items-center justify-center border border-solid rounded-lg p-2 w-full cursor-pointer hover:bg-[color:rgb(var(--v-theme-primary--hover))] active:bg-[color:rgb(var(--v-theme-primary--hover))]"
        @click="click(item)">
        <div
          class="text-2xl font-bold text-[color:rgb(var(--sv-text-title-primary))] group-hover:text-[color:rgb(var(--sv-text-fg-primary--hover))]">
          {{ item.title }}</div>
        <div class="text-xl font-semibold">{{ item.value }}</div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { DashboardItem } from "@/access";
import api from "@/api";
import { onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";

const { t } = useI18n({ useScope: 'global' });
const router = useRouter();

const items = ref<DashboardItem[]>([]);

const click = (item: DashboardItem) => {
  router.push({ name: item.route })
}

const addItem = (title: string, value: number, route: string) => {
  items.value.push({ title: title, value: value, route: route });
}

const refresh = () => {
  api.get("v1/statistics/dashboard")
    .then(response => {
      addItem(t("permissions"), response.data.permissionCount, "permissions");
      addItem(t("identities"), response.data.identityCount, "identities");
      addItem(t("roles"), response.data.roleCount, "roles");
    })
}

onMounted(() => {
  refresh();
})
</script>
