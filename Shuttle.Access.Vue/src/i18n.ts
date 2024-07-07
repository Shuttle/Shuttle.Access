import { nextTick } from "vue";
import type { I18n, Locale } from "vue-i18n";

export async function loadLocaleMessages(i18n: I18n, locale: Locale) {
  const messages = await import(`./locales/${locale}.json`);

  i18n.global.setLocaleMessage(locale, messages.default);

  return nextTick();
}
