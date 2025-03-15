<template>
  <div class="flex flex-row align-middle bsc-title items-start">
    <div class="flex-grow">{{ $t(props.title) }}</div>
    <v-btn v-if="props.closePath" :icon="mdiClose" @click.stop="router.push(props.closePath)" size="small" flat></v-btn>
  </div>
</template>

<script setup lang="ts">
import type { FormTitle } from '@/access'
import { useRouter } from 'vue-router'
import { mdiClose } from '@mdi/js'

const router = useRouter()
const props = defineProps<FormTitle>()
</script>
