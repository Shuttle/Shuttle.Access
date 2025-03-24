import type { SnackbarStoreState } from "@/access";
import { defineStore } from "pinia";

export const useSnackbarStore = defineStore("snackbar", {
  state: (): SnackbarStoreState => {
    return {
      visible: false,
      text: "",
      timeout: 2000,
    };
  },
  actions: {
    open(text: string, timeout: number = 2000) {
      this.text = text;
      this.visible = true;
      this.timeout = timeout;
    },
    close() {
      this.visible = false;
    },
  },
});
