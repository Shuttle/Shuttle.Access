import type { Alert } from "@/access";
import { defineStore } from "pinia";
import { i18n } from "@/i18n";

let key = 0;

export const useAlertStore = defineStore("alert", () => {
  const alerts = ref<Alert[]>([]);

  const add = (alert: Alert) => {
    if (!alert || !alert.message) {
      return;
    }

    remove(alert.name);

    alert.expire = alert.expire ?? true;
    alert.expirySeconds = alert.expirySeconds ?? 10;
    alert.dismissable = alert.dismissable || !!alert.name;

    alert.key = `${alert.name}-${key++}`;

    alerts.value.push(alert);

    if (alert.expire) {
      setTimeout(() => {
        remove(alert.name);
      }, alert.expirySeconds * 1000);
    }
  };

  const remove = (name: string) => {
    if (!name) {
      return false;
    }

    const index = alerts.value.findIndex((item) => item.name === name);

    if (index < 0) {
      return false;
    }

    alerts.value.splice(index, 1);

    return true;
  };

  const clear = () => {
    alerts.value = [];
  };

  const requestSent = () => {
    add({
      message: i18n.global.t("system-messages.request-sent"),
      name: "request-sent",
    });
  };

  const working = () => {
    add({
      message: i18n.global.t("system-messages.working"),
      name: "working-message",
    });
  };

  return {
    alerts: readonly(alerts),
    add,
    remove,
    clear,
    requestSent,
    working,
  };
});
