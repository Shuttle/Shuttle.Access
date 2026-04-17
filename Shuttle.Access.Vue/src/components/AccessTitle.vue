<template>
  <div class="flex flex-row align-middle items-start font-semibold text-xl pb-2 mb-4 w-full cursor-default"
    :class="getClasses()">
    <div class="grow">{{ props.title }}</div>
    <v-btn v-if="canClose" :icon="mdiClose" @click.stop="click" size="x-small" flat></v-btn>
  </div>
</template>

<script setup lang="ts">
import type { FormTitle } from '@/access'
import { useRouter } from 'vue-router'
import { useDrawerStore } from '@/stores/drawer'
import { mdiClose } from '@mdi/js'

const router = useRouter()
const props = defineProps<FormTitle>()

const canClose = props.closeDrawer || props.closePath || props.closeClick

const getClasses = () => {
  switch (props.type) {
    case "borderless": {
      return "border-0"
    }
    default: {
      return "border-b-1 border-b-solid border-b-neutral-200 dark:border-b-neutral-500"
    }
  }
}

const click = () => {
  if (props.closeDrawer) {
    useDrawerStore().close(false)
    return
  }

  if (props.closePath) {
    router.push(props.closePath)
    return
  }

  if (props.closeClick) {
    props.closeClick()
  }
}
</script>
