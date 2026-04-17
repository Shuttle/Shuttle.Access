<template>
  <div class="mx-auto ease-in duration-300 bg-neutral-100 dark:bg-neutral-800" :class="getClasses()">
    <slot></slot>
  </div>
</template>

<script lang="ts" setup>
const props = withDefaults(
  defineProps<{ showBorder?: boolean, size?: "small" | "medium" | "large" | "full" }>(),
  {
    showBorder: false,
    size: "full"
  }
)

const getClasses = () => {
  const classes: string[] = [];

  if (props.showBorder) {
    classes.push("rounded-md p-2 my-2 border border-neutral-300 dark:border-neutral-700");
  }

  switch (props.size) {
    case "small": {
      classes.push("lg:w-1/4 md:w-2/4");
      break;
    }
    case "medium": {
      classes.push("lg:w-2/4 md:w-3/4");
      break;
    }
    case "large": {
      classes.push("lg:w-3/4");
      break;
    }
    default: {
      classes.push("w-full");
    }
  }

  return classes;
}
</script>
