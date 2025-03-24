import type { Status } from "@/access";
import { useI18n } from "vue-i18n";

export function usePermissionStatuses(): Status[] {
  const { t } = useI18n({ useScope: "global" });

  return [
    {
      text: t("active"),
      value: 1,
    },
    {
      text: t("deactivated"),
      value: 2,
    },
    {
      text: t("removed"),
      value: 3,
    },
  ];
}
