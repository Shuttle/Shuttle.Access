import { createApp } from "vue";
import { registerPlugins } from "@/plugins";

import App from "./App.vue";
import router from "./router";

import { useSessionStore } from "@/stores/session";

const app = createApp(App);

registerPlugins(app);

const sessionStore = useSessionStore();

if (window.location.pathname !== "/oauth") {
  try {
    await sessionStore.initialize();
  } catch {
    if (!window.location.pathname.startsWith("/signin")) {
      router.push({ path: "/signin" });
    }
  }
}

if (
  window.location.pathname === "/" ||
  window.location.pathname === "/signin"
) {
  router.push({ path: sessionStore.authenticated ? "/dashboard" : "/signin" });
}

app.mount("#app");
