import { createApp } from "vue";
import { registerPlugins } from "@/plugins";

import App from "./App.vue";
import router from "./router";
import { loadLocaleMessages } from "@/i18n";

import { useAlertStore } from "@/stores/alert";
import { useSessionStore } from "@/stores/session";
import { createI18n } from "vue-i18n";

const app = createApp(App);

const i18n = createI18n({
  legacy: false,
  locale: "en",
  fallbackLocale: "en"
});

i18n.global.locale.value = "en";

document.querySelector("html")?.setAttribute("lang", i18n.global.locale.value);

await loadLocaleMessages(i18n, "en");

app.use(i18n);
app.use(router);

registerPlugins(app);

const alertStore = useAlertStore();
const sessionStore = useSessionStore();

await sessionStore.initialize().catch((error) => {
  alertStore.add({
    message: i18n.global.t("exceptions.session-initialize", {
      error: error.toString(),
    }),
    type: "error",
    name: "session-initialize",
  });

  router.push({ path: "/signin" });
});

if (window.location.pathname === "/") {
  router.push({ path: sessionStore.authenticated ? "/dashboard" : "/signin" });
}

app.mount("#app");
