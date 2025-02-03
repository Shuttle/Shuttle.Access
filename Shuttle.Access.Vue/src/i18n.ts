import { nextTick } from "vue";
import type { I18n, Locale } from "vue-i18n";
import { createI18n } from "vue-i18n";

export const i18n = createI18n({
  legacy: false,
  locale: "en",
  fallbackLocale: "en",
});

i18n.global.locale.value = "en";

export async function loadLocaleMessages(i18n: I18n, locale: Locale) {
  const messages = await import(`./locales/${locale}.json`);

  i18n.global.setLocaleMessage(locale, messages.default);

  return nextTick();
}
