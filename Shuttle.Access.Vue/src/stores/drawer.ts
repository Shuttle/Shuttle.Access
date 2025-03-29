import type { DrawerOptions } from "@/access";
import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { useRoute, useRouter } from "vue-router";

export const useDrawerStore = defineStore("drawer", () => {
  const drawerOptions = ref<DrawerOptions>({
    parentPath: "",
    refresh: (): Promise<void> => Promise.resolve(),
  });

  const route = useRoute();
  const router = useRouter();

  const isOpen = computed(() => {
    // `matched` should include the parent route and the current route in order to open the drawer.
    return (
      route.matched.length > 1 &&
      !(route?.fullPath ?? "").endsWith(drawerOptions.value?.parentPath)
    );
  });

  const initialize = (options: DrawerOptions) => {
    if (!options) {
      throw new Error("The 'options' argument may not be undefined.");
    }

    drawerOptions.value = options;
  };

  async function close(refresh = true) {
    router.push(drawerOptions.value.parentPath);

    if (refresh) {
      await drawerOptions.value.refresh();
    }
  }

  return {
    options: drawerOptions,
    isOpen: isOpen,
    close,
    initialize,
  };
});
