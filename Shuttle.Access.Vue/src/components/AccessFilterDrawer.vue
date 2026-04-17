<template>
  <v-navigation-drawer v-if="drawerStore.filterDrawerVisible" location="left" width="350" class="p-4">
    <form class="flex flex-col gap-2 mb-2" @submit.prevent="emit('filter')">
      <slot></slot>
      <s-strip reverse>
        <v-btn-primary :append-icon="mdiFilterOutline" type="submit">{{
          $t('filter')
        }}</v-btn-primary>
      </s-strip>
    </form>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { mdiFilterOutline } from '@mdi/js'
import { useDrawerStore } from '@/stores/drawer'

const emit = defineEmits(['filter'])
const drawerStore = useDrawerStore()
</script>
