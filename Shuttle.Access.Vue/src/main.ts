import { createApp } from "vue";
import { registerPlugins } from "@/plugins";

import App from "./App.vue";
import router from "./router";

import { useSessionStore } from "@/stores/session";
import { useAlertStore } from "./stores/alert";

const app = createApp(App);

registerPlugins(app);

const sessionStore = useSessionStore();

if (!sessionStore.isInitialized && window.location.pathname !== "/oauth") {
  try {
    await sessionStore.initialize();
  } catch (error: any) {
    useAlertStore().add({
      message: error.toString(),
      type: "error",
      name: "session-initialize",
    });
    if (!window.location.pathname.startsWith("/sign-in")) {
      router.push({ path: "/sign-in" });
    }
  }
}

if (
  window.location.pathname === "/" ||
  window.location.pathname === "/sign-in"
) {
  router.push({
    path: sessionStore.isAuthenticated ? "/dashboard" : "/sign-in",
  });
}

app.mount("#app");
