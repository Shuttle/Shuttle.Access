import type { DrawerOptions, DrawerSize } from "@/access";
import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useConfirmationStore } from "@/stores/confirmation";

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
  const showProfileDrawer = ref(false);
  const filterDrawerVisible = ref(true);

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

  const close = async (refresh = true) => {
    const confirmationStore = useConfirmationStore();

    if (!(await confirmationStore.confirmClose())) {
      return;
    }

    router.push(drawerOptions.value.parentPath);

    if (refresh) {
      await drawerOptions.value.refresh();
    }
  };

  const toggleFilterDrawer = () => {
    filterDrawerVisible.value = !filterDrawerVisible.value;
  };

  const refresh = async () => {
    await drawerOptions.value.refresh();
  };

  return {
    showNavigationDrawer,
    showProfileDrawer,
    options: drawerOptions,
    sizeToggleVisible,
    size,
    setSize,
    isOpen,
    close,
    initialize,
    filterDrawerVisible,
    toggleFilterDrawer,
    refresh,
  };
});
