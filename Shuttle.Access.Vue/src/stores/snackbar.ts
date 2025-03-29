import type { SnackbarStoreState } from "@/access";
import { defineStore } from "pinia";
import { i18n } from "@/i18n";

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
    requestSent(timeout: number = 3000) {
      this.open(i18n.global.t("messages.request-sent"), timeout);
    },
    working(timeout: number = 3000) {
      this.open(i18n.global.t("messages.working"), timeout);
    },
  },
});
