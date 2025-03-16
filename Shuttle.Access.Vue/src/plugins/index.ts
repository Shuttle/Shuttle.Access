/**
 * plugins/index.ts
 *
 * Automatically included in `./src/main.ts`
 */

// Plugins
import vuetify from "./vuetify";
import pinia from "@/stores";
import router from "@/router";

// Types
import type { App } from "vue";

// Components
import FormTitle from "@/components/FormTitle.vue";

export function registerPlugins(app: App) {
  app.use(vuetify).use(router).use(pinia);

  app.component("sv-title", FormTitle);
}
