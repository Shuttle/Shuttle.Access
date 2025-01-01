import type { ConfirmationOptions, ConfirmationStoreState } from "@/access";
import { defineStore } from "pinia";

export const useConfirmationStore = defineStore("confirmation", {
  state: (): ConfirmationStoreState => {
    return {
      isOpen: false,
      options: undefined,
    };
  },
  actions: {
    show(options: ConfirmationOptions) {
      if (!options) {
        throw new Error("The 'options' argument may not be undefined.");
      }

      this.options = options;
      this.isOpen = true;
    },
    close() {
      this.isOpen = false;
    },
    confirmed() {
      this.options?.onConfirm?.(this.options.item);
    },
  },
});
