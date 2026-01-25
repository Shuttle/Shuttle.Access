import type { DrawerOptions, DrawerSize } from "@/access";
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

  const size = ref<DrawerSize>("compact");
  const sizeToggleVisible = ref(false);
  const showNavigationDrawer = ref(false);

  const setSize = (drawerSize: DrawerSize) => {
    size.value = drawerSize;
  };

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
    showNavigationDrawer,
    options: drawerOptions,
    sizeToggleVisible,
    size,
    setSize,
    isOpen,
    close,
    initialize,
  };
});
