<template>
  <div v-if="drawerStore.isOpen"
    :class="drawerStore.size != 'full' ? 'fixed top-0 right-0 bottom-0 w-full z-1000' : ''">
    <div v-if="drawerStore.size !== 'full'" class="h-full w-full bg-black opacity-25"
      @click.self="drawerStore.close(false)"></div>
    <div class="opacity-100" :class="getClasses()" @click.stop>
      <v-btn v-if="drawerStore.sizeToggleVisible" :icon="getToggleIcon()" @click.stop="toggle" size="small" flat
        class="absolute top-4 -left-10 border rounded-r-none z-5000"></v-btn>
      <v-btn v-if="drawerStore.sizeToggleVisible" :icon="mdiFullscreen" @click.stop="full" size="small" flat
        class="absolute top-16 -left-10 border rounded-r-none z-5000"></v-btn>
      <div class="overflow-y-auto">
        <router-view></router-view>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useDrawerStore } from '@/stores/drawer'
import { mdiFullscreen, mdiUnfoldLessVertical, mdiUnfoldMoreVertical } from '@mdi/js'

const drawerStore = useDrawerStore()

const getToggleIcon = () => {
  return drawerStore.size === 'expanded' ? mdiUnfoldLessVertical : mdiUnfoldMoreVertical
}

const full = () => {
  drawerStore.setSize('full')
}

const toggle = () => {
  drawerStore.setSize(drawerStore.size === 'compact' ? 'expanded' : 'compact')
}

const getClasses = () => {
  let classes: string = 'w-full'

  switch (drawerStore.size) {
    case 'compact':
      classes += ' md:w-1/3'
      break
    case 'expanded':
      classes += ' md:w-2/3'
      break
  }

  if (drawerStore.size !== 'full') {
    classes += ' v-navigation-drawer v-navigation-drawer--right right-0 top-0 border-t-1 p-2'
  }

  return classes
}

watch(
  () => drawerStore.isOpen,
  (newVal) => {
    if (newVal) {
      drawerStore.size = 'compact'
      document.documentElement.classList.add('overflow-hidden')
    } else {
      drawerStore.size = undefined
      document.documentElement.classList.remove('overflow-hidden')
    }
  },
)

watch(
  () => drawerStore.size,
  (newVal) => {
    switch (newVal) {
      case 'compact':
      case 'expanded':
        document.documentElement.classList.add('overflow-hidden')
        break
      default:
        document.documentElement.classList.remove('overflow-hidden')
        break
    }
  },
)

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
