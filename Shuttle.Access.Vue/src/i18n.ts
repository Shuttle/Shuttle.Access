import { createI18n } from "vue-i18n";
import { en } from "vuetify/locale";
import enMessages from "./locales/en.json";

export const i18n = createI18n({
  legacy: false,
  locale: "en-GB",
  fallbackLocale: "en",
});

i18n.global.locale.value = "en-GB";

i18n.global.setLocaleMessage("en", enMessages);
i18n.global.setLocaleMessage("en-GB", {
  $vuetify: {
    ...en,
    Intl: {
      DateTimeFormat: "dd/mm/yyyy",
    },
  },
});
