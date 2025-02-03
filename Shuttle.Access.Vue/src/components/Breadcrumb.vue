<template>
  <v-breadcrumbs :items="items" class="pt-0" density="compact">
    <template v-slot:item="{ item, index }">
      <v-breadcrumbs-item :key="index" :disabled="item.disabled" @click="navigateTo(item.href!, index)"
        class="cursor-pointer select-none border border-solid rounded-full px-2 opacity-40 hover:opacity-80">
        {{ item.title }}
      </v-breadcrumbs-item>
    </template>
  </v-breadcrumbs>
</template>

<script setup lang="ts">
import type { Breadcrumb } from '@/access';
import { useBreadcrumbStore } from '@/stores/breadcrumb';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import type { VBreadcrumbsItem } from 'vuetify/components';

const { t } = useI18n({ useScope: 'global' });

const breadcrumbStore = useBreadcrumbStore();
const router = useRouter();

type BreadcrumbItem = VBreadcrumbsItem['$props']

const items: ComputedRef<any[]> = computed((): BreadcrumbItem[] => {
  const length = breadcrumbStore.breadcrumbs.length;
  return breadcrumbStore.breadcrumbs
    .filter((_breadcrumb: Breadcrumb, index: number) => {
      return index < length - 1;
    })
    .map((breadcrumb: Breadcrumb, index: number) => {
      return {
        title: t(breadcrumb.name ?? "unknown"),
        href: breadcrumb.path!,
        disabled: false
      }
    });
});

function navigateTo(path: string, index: number) {
  breadcrumbStore.removeBreadcrumbsAfter(index);
  router.push(path);
}
</script>
