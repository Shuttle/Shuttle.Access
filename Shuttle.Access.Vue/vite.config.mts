// Plugins
import Components from "unplugin-vue-components/vite";
import Vue from "@vitejs/plugin-vue";
import vueDevTools from "vite-plugin-vue-devtools";
import Vuetify, { transformAssetUrls } from "vite-plugin-vuetify";
import ViteFonts from "unplugin-fonts/vite";
import AutoImport from "unplugin-auto-import/vite";
import tailwindcss from "@tailwindcss/vite";

// Utilities
import { defineConfig } from "vite";
import { fileURLToPath, URL } from "node:url";

// https://vitejs.dev/config/
export default defineConfig({
  esbuild: {
    supported: {
      "top-level-await": true,
    },
  },
  plugins: [
    Vue({
      template: { transformAssetUrls },
    }),
    vueDevTools(),
    Vuetify({
      autoImport: true,
      styles: {
        configFile: "src/styles/settings.scss",
      },
    }),
    Components(),
    ViteFonts({
      google: {
        families: [
          {
            name: "Roboto",
            styles: "wght@100;300;400;500;700;900",
          },
        ],
      },
    }),
    AutoImport({
      imports: [
        "vue",
        {
          "vue-router/auto": ["useRoute", "useRouter"],
        },
      ],
      dts: "src/auto-imports.d.ts",
      eslintrc: {
        enabled: true,
      },
      vueTemplate: true,
    }),
    tailwindcss(),
  ],
  define: { "process.env": {} },
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
    extensions: [".js", ".json", ".jsx", ".mjs", ".ts", ".tsx", ".vue"],
  },
  server: {
    port: 3000,
  },
  css: {
    preprocessorOptions: {
      sass: {
        api: "modern-compiler",
      },
    },
  },
});
