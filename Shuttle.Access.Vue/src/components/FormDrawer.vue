<template>
  <div v-if="drawerStore.isOpen" class="sv-form-drawer">
    <div class="sv-form-drawer__overlay" @click.self="drawerStore.close(false)"></div>
    <div
      class="v-navigation-drawer v-navigation-drawer--right fixed right-0 bottom-0 top-[var(--v-layout-top)] border-t-1 z-[1001] p-2 overflow-y-scroll opacity-100"
      :class="getClasses()" @click.stop>
      <router-view></router-view>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useDrawerStore } from '@/stores/drawer'

const drawerStore = useDrawerStore()

const getClasses = () => {
  return 'w-full md:w-1/3'
}

const handleKeydown = (event: KeyboardEvent) => {
  if (event.key !== 'Escape') {
    return
  }

  drawerStore.close(false)
}

onMounted(() => {
  window.addEventListener('keydown', handleKeydown)
})

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeydown)
})
</script>
