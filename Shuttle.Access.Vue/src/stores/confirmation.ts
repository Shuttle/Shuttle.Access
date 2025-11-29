import { defineStore } from "pinia";
import { ref } from "vue";
import type { ConfirmationOptions } from "@/access";

export const useConfirmationStore = defineStore("confirmation", () => {
  const isOpen = ref(false);
  const options = ref<ConfirmationOptions | undefined>(undefined);

  function show(data: ConfirmationOptions) {
    if (!data) {
      throw new Error("The 'options' argument may not be undefined.");
    }
    options.value = data;
    isOpen.value = true;
  }

  function close() {
    isOpen.value = false;
  }

  function confirmed() {
    options.value?.onConfirm?.(options.value.item);
  }

  return {
    isOpen,
    options,
    show,
    close,
    confirmed,
  };
});
