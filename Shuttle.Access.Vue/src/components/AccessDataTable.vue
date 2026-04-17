<template>
  <v-data-table :hide-default-footer="props.itemsPerPage > 0 && (props.items?.length ?? 0) < props.itemsPerPage"
    :items="props.items" :items-per-page="props.itemsPerPage" v-bind="$attrs">
    <template v-for="(_, name) in $slots" v-slot:[name]="slotData">
      <slot :name="name" v-bind="slotData"></slot>
    </template>
  </v-data-table>
</template>

<script lang="ts" setup>
type Props = {
  items?: any[];
  itemsPerPage?: number;
};

defineOptions({
  inheritAttrs: false
})

const props = withDefaults(
  defineProps<Props>(),
  {
    items: () => [],
    itemsPerPage: 10
  }
)
</script>
