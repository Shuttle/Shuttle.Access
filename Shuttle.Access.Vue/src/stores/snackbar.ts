import { defineStore } from "pinia";
import { ref } from "vue";
import { i18n } from "@/i18n";

export const useSnackbarStore = defineStore("snackbar", () => {
  const visible = ref(false);
  const text = ref("");
  const timeout = ref(2000);

  function open(message: string, duration: number = 2000) {
    text.value = message;
    visible.value = true;
    timeout.value = duration;
  }

  function close() {
    visible.value = false;
  }

  function requestSent(duration: number = 3000) {
    open(i18n.global.t("messages.request-sent"), duration);
  }

  function working(duration: number = 3000) {
    open(i18n.global.t("messages.working"), duration);
  }

  return {
    visible,
    text,
    timeout,
    open,
    close,
    requestSent,
    working,
  };
});
