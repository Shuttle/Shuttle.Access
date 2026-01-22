<template>
  <div v-if="drawerStore.isOpen" class="sv-form-drawer">
    <div class="sv-form-drawer__overlay" @click.self="drawerStore.close(false)"></div>
    <div
      class="v-navigation-drawer v-navigation-drawer--right right-0 top-0 border-t-1 p-2"
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
