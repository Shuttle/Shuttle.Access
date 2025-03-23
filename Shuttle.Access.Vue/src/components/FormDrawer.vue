<template>
  <div class="sv-form-drawer" @click="router.push(props.closePath)">
    <div
      class="v-navigation-drawer v-navigation-drawer--right fixed right-0 bottom-0 top-[var(--v-layout-top)] border-t-1 z-[1001] p-2"
      :class="getClasses()" @click.stop>
      <sv-title v-if="props.title" :title="props.title" :close-path="props.closePath" type="borderless" />
      <router-view></router-view>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { FormDrawer } from '@/access'
import { useRouter } from 'vue-router'

const router = useRouter()
const props = defineProps<FormDrawer>()

const getClasses = () => {
  return "w-1/3"
}

const handleKeydown = (event: KeyboardEvent) => {
  if (event.key !== "Escape") {
    return
  }

  router.push(props.closePath)
};

onMounted(() => {
  window.addEventListener("keydown", handleKeydown);
});

onUnmounted(() => {
  window.removeEventListener("keydown", handleKeydown);
});
</script>
