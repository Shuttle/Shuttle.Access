import { defineStore } from "pinia";

export interface ConfirmationStoreState {
  item: any;
  isOpen: boolean;
  callback?: (item: any) => void;
}

export const useConfirmationStore = defineStore("confirmation", {
  state: (): ConfirmationStoreState => {
    return {
      item: undefined,
      isOpen: false,
      callback: undefined,
    };
  },
  actions: {
    show(item: any, callback: (item: any) => void) {
      if (!item) {
        throw new Error("The 'item' argument may not be undefined.");
      }

      this.item = item;
      this.callback = callback;

      this.setIsOpen(true);
    },
    setIsOpen(open: boolean) {
      this.isOpen = open;
    },
    confirmed() {
      this.callback?.(this.item);
    },
  },
});
