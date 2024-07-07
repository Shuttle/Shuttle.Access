import { ref } from "vue";
import { useSessionStore } from "@/stores/session";

const sessionStore = useSessionStore();

export function useSecureTableFields(fields) {
    const securedFields = ref([])

    fields.forEach(item => {
        if (!item.permission || sessionStore.hasPermission(item.permission)) {
            securedFields.value.push(item);
        }
    });

    return securedFields;
}