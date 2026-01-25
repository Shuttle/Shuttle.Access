<template>
  <div class="flex flex-row items-center gap-2" :class="getClasses()">
    <slot></slot>
  </div>
</template>

<script lang="ts" setup>
type Props = {
  reverse?: boolean
};

const props = withDefaults(
  defineProps<Props>(),
  {
    reverse: false
  }
)

const getClasses = () => {
  return props.reverse ? "flex-row-reverse" : "";
}
</script>
