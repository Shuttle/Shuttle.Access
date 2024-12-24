import { ref } from "vue";
import { useSessionStore } from "@/stores/session";

const sessionStore = useSessionStore();

export function useSecureTableHeaders(fields: any) {
  const securedFields: Ref<any[]> = ref([]);

  fields.forEach((item: any) => {
    if (!item.permission || sessionStore.hasPermission(item.permission)) {
      securedFields.value.push(item);
    }
  });

  return securedFields;
}
