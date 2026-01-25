/**
 * plugins/vuetify.ts
 *
 * Framework documentation: https://vuetifyjs.com`
 */

// Styles
import "@mdi/font/css/materialdesignicons.css";
import "vuetify/styles";
import { aliases, mdi } from "vuetify/iconsets/mdi-svg";
import { createVueI18nAdapter } from "vuetify/locale/adapters/vue-i18n";
import { i18n } from "@/i18n";

// Components
import { VDateInput } from "vuetify/labs/VDateInput";
import { VFileUpload } from "vuetify/labs/components";

// Composables
import { createVuetify, type ThemeDefinition } from "vuetify";
import { VBtn } from "vuetify/components/VBtn";
import { useI18n } from "vue-i18n";

const colors = {
  primary: "#F97316",
  "primary--hover": "#fa7d24",
  "primary--active": "#eb7826",
  "primary-text--hover": "#f9f9f9",
  "primary-text--active": "#d1d1d1",
  secondary: "#444444",
  "secondary-darken-1": "#383838",
  error: "#bb4445",
  info: "#2196F3",
  success: "#4CAF50",
  warning: "#FB8C00",
};

const darkTheme: ThemeDefinition = {
  dark: true,
  colors: colors,
};

const lightTheme: ThemeDefinition = {
  dark: false,
  colors: colors,
};

// https://vuetifyjs.com/en/introduction/why-vuetify/#feature-guides
export default createVuetify({
  components: {
    VDateInput,
    VFileUpload,
  },
  aliases: {
    VBtnPrimary: VBtn,
  },
  defaults: {
    VBtn: {
      variant: "flat",
    },
    VBtnPrimary: {
      variant: "tonal",
      color: "primary",
    },
  },
  locale: {
    adapter: createVueI18nAdapter({ i18n, useI18n }),
  },
  theme: {
    defaultTheme: "dark",
    themes: {
      dark: darkTheme,
      light: lightTheme,
    },
  },
  icons: {
    defaultSet: "mdi",
    aliases,
    sets: {
      mdi,
    },
  },
});
