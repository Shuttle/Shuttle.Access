<template>
  <sv-form-layout :title="!!id ? $t('tenant') : $t('new-tenant')" @close="close">
    <p>Tenant create/edit form placeholder</p>
  </sv-form-layout>
</template>

<script setup lang="ts">
import { useDrawerStore } from "@/stores/drawer";
import { onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";

const props = defineProps<{
  id?: string;
}>();

const { t } = useI18n({ useScope: 'global' });
const drawerStore = useDrawerStore();

const close = () => {
  drawerStore.close();
};
</script>
