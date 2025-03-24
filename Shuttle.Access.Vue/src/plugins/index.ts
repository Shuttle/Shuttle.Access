/**
 * plugins/index.ts
 *
 * Automatically included in `./src/main.ts`
 */

// Plugins
import vuetify from "./vuetify";
import pinia from "@/stores";
import router from "@/router";
import { i18n } from "@/i18n";
import "@/styles/base.css";

// Types
import type { App } from "vue";

// Components
import FormTitle from "@/components/FormTitle.vue";
import FormDrawer from "@/components/FormDrawer.vue";

document.querySelector("html")?.setAttribute("lang", i18n.global.locale.value);

export function registerPlugins(app: App) {
  app.use(vuetify).use(router).use(pinia).use(i18n);

  app.component("sv-title", FormTitle);
  app.component("sv-form-drawer", FormDrawer);
}
