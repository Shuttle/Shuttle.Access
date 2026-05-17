<template>
  <a-container size="small" class="p-4">
    <a-title :title="$t('tenant-selection')"></a-title>
    <div class="flex flex-col gap-2">
      <v-btn v-for="tenant in sessionStore.tenants" v-bind:key="tenant.id" :alt="`${tenant.id} logo`"
        class="py-8 px-4 flex flex-row justify-center items-center gap-2 w-full border cursor-pointer"
        @click="select(tenant)">
        <div v-if="tenant.logoSvg" v-html="tenant.logoSvg" class="v-icon__svg w-8 h-8 mr-4"></div>
        <span>{{ tenant.name }}</span>
      </v-btn>
    </div>
  </a-container>
</template>

<script setup lang="ts">
import { useSessionStore } from "@/stores/session";
import router from "@/router";
import type { Tenant } from "@/access";

const sessionStore = useSessionStore();

const select = async (tenant: Tenant) => {
  sessionStore.selectTenantId(tenant.id);

  router.push({ name: "dashboard" });
}
</script>
