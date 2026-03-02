import { i18n } from "@/i18n";
import type {
  ConfirmationItem,
  ConfirmationOptions,
  ConfirmationResult,
} from "@/access";
import { defineStore } from "pinia";

export const useConfirmationStore = defineStore("confirmation", () => {
  let promise: Promise<ConfirmationResult>;
  let resolved!: (
    value: ConfirmationResult | PromiseLike<ConfirmationResult>,
  ) => void;
  let rejected!: (reason?: any) => void;
  const confirmationItems: Ref<ConfirmationItem[]> = ref([]);
  const isOpen = ref(false);
  const options = ref<ConfirmationOptions>({});

  const show = async (
    opts: ConfirmationOptions,
  ): Promise<ConfirmationResult> => {
    options.value = opts;
    isOpen.value = true;

    promise = new Promise((resolve, reject) => {
      resolved = resolve;
      rejected = reject;
    });

    return new Promise(async (resolve) => {
      try {
        await promise;

        resolve({
          confirmed: true,
          item: options.value?.item,
        });
      } catch {
        resolve({
          confirmed: false,
          item: options.value?.item,
        });
      }
    });
  };

  const close = async () => {
    if (options.value) {
      rejected();
    }
    isOpen.value = false;
  };

  const confirmed = async () => {
    if (options.value) {
      resolved(options.value.item);
    }
    isOpen.value = false;
  };

  const touch = (name: string) => {
    const item = confirmationItems.value?.find((item) => item.name === name);

    if (item) {
      item.touched = true;
    }
  };

  const touched = (name: string) => {
    return (
      confirmationItems.value?.find((item) => item.name === name)?.touched ??
      false
    );
  };

  const addConfirmationState = (name: string, state: any) => {
    if (confirmationItems.value.some((other) => other.name === name)) {
      removeConfirmationItem(name);
    }

    confirmationItems.value.push({
      name,
      getConfirmationMessage: () => {
        return touched(name)
          ? i18n.global.t("_system-messages.confirm-close")
          : "";
      },
      unwatch: watch(
        state,
        () => {
          touch(name);
        },
        { deep: true, once: true },
      ),
    });
  };

  const addConfirmationItem = (
    name: string,
    getConfirmationMessage: () => string,
  ) => {
    if (confirmationItems.value.some((other) => other.name === name)) {
      removeConfirmationItem(name);
    }

    confirmationItems.value.push({
      name,
      getConfirmationMessage,
    });
  };

  const removeConfirmationItem = (name: string) => {
    const item = confirmationItems.value.find((other) => other.name === name);

    if (!item) {
      return;
    }

    item.unwatch?.();
    confirmationItems.value = confirmationItems.value.filter(
      (other) => other.name !== name,
    );
  };

  const getConfirmationMessage = () => {
    return (
      confirmationItems.value[
        confirmationItems.value.length - 1
      ]?.getConfirmationMessage() ?? ""
    );
  };

  const confirmClose = async () => {
    const message = getConfirmationMessage();

    if (message) {
      const response = await show({ messageText: message });

      if (!response.confirmed) {
        return false;
      }
    }

    confirmationItems.value.pop()?.unwatch?.();

    return true;
  };

  return {
    addConfirmationItem,
    addConfirmationState,
    close,
    confirmClose,
    confirmed,
    getConfirmationMessage,
    isOpen,
    options,
    removeConfirmationItem,
    show,
  };
});
