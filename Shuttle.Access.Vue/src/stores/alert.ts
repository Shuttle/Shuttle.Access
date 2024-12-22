import { defineStore } from "pinia";

export interface Alert {
  message: string;
  name: string;
  type?: "error" | "success" | "warning" | "info" | undefined;
  expire?: boolean;
  expirySeconds?: number;
  dismissable?: boolean;
  key?: string;
}

export interface AlertStoreState {
  alerts: Alert[];
}

export const messages = {
  requestSent:
    "The request has been submitted.  It may take a moment to process.",
  working:
    "Please wait until the current operation completes and then try again.",
};

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
      alert.dismissable = alert.dismissable || !!alert.name;

      if (alert.expire) {
        alert.expirySeconds = 10;
      }

      alert.key = `${alert.name}-${key++}`;

      this.alerts.push(alert);
    },
    remove(name: String) {
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
    requestSent() {
      this.add({
        message: messages.requestSent,
        name: "request-sent",
      });
    },
    working() {
      this.add({
        message: messages.working,
        name: "working-message",
      });
    },
  },
});
