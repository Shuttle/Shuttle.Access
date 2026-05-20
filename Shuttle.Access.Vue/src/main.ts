import { createApp } from "vue";
import { registerPlugins } from "@/plugins";

import App from "./App.vue";
import router from "./router";

import { useSessionStore } from "@/stores/session";

const app = createApp(App);

registerPlugins(app);

const sessionStore = useSessionStore();

if (
  window.location.pathname === "/" ||
  window.location.pathname === "/sign-in"
) {
  router.push({
    path: sessionStore.isAuthenticated ? "/dashboard" : "/sign-in",
  });
}

app.mount("#app");
