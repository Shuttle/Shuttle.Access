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
import AccessContainer from "@/components/AccessContainer.vue";
import AccessDataTable from "@/components/AccessDataTable.vue";
import AccessDrawer from "@/components/AccessDrawer.vue";
import AccessTitle from "@/components/AccessTitle.vue";
import AccessStrip from "@/components/AccessStrip.vue";

document.querySelector("html")?.setAttribute("lang", i18n.global.locale.value);

export function registerPlugins(app: App) {
  app.use(vuetify).use(router).use(pinia).use(i18n);

  app.component("a-container", AccessContainer);
  app.component("a-data-table", AccessDataTable);
  app.component("a-drawer", AccessDrawer);
  app.component("a-strip", AccessStrip);
  app.component("a-title", AccessTitle);
}
