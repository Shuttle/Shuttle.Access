import type { Alert, AlertStoreState } from "@/access";
import { defineStore } from "pinia";

let key = 0;

export const useAlertStore = defineStore("alert", {
  state: (): AlertStoreState => {
    return {
      alerts: [],
    };
  },
  actions: {
    add(alert: Alert) {
      if (!alert || !alert.message) {
        return;
      }

      this.remove(alert.name);

      alert.expire = alert.expire ?? true;
      alert.expirySeconds = alert.expirySeconds ?? 10;
      alert.dismissable = alert.dismissable || !!alert.name;

      alert.key = `${alert.name}-${key++}`;

      this.alerts.push(alert);

      if (alert.expire) {
        setTimeout(() => {
          this.remove(alert.name);
        }, alert.expirySeconds * 1000);
      }
    },
    remove(name: string) {
      if (!name) {
        return false;
      }

      const index = this.alerts.findIndex((item) => item.name === name);

      if (index < 0) {
        return false;
      }

      this.alerts.splice(index, 1);

      return true;
    },
    clear() {
      this.alerts = [];
    },
  },
});
