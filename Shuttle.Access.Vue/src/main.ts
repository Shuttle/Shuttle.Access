import { createApp } from "vue";
import { registerPlugins } from "@/plugins";

import App from "./App.vue";
import router from "./router";
import { loadLocaleMessages } from "@/i18n";

import { useAlertStore } from "@/stores/alert";
import { useSessionStore } from "@/stores/session";
import { i18n } from "@/i18n";

const app = createApp(App);

document.querySelector("html")?.setAttribute("lang", i18n.global.locale.value);

await loadLocaleMessages(i18n, "en");

app.use(i18n);
app.use(router);

registerPlugins(app);

const sessionStore = useSessionStore();

if (window.location.pathname !== "/oauth") {
  await sessionStore.initialize().catch((error) => {
    if (!window.location.pathname.startsWith("/signin")) {
      router.push({ path: "/signin" });
    }
  });
}

if (
  window.location.pathname === "/" ||
  window.location.pathname === "/signin"
) {
  router.push({ path: sessionStore.authenticated ? "/dashboard" : "/signin" });
}

app.mount("#app");
