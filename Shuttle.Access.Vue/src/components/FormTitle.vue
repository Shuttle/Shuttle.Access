<template>
  <div class="flex flex-row align-middle sv-title items-start" :class="getClasses()">
    <div class="flex-grow">{{ props.title }}</div>
    <v-btn v-if="props.closePath" :icon="mdiClose" @click.stop="router.push(props.closePath)" size="x-small"
      flat></v-btn>
  </div>
</template>

<script setup lang="ts">
import type { FormTitle } from '@/access'
import { useRouter } from 'vue-router'
import { mdiClose } from '@mdi/js'

const router = useRouter()
const props = defineProps<FormTitle>()

const getClasses = () => {
  switch (props.type) {
    case "borderless": {
      return "sv-title--borderless"
    }
    default: {
      return "sv-title--normal"
    }
  }
}
</script>
