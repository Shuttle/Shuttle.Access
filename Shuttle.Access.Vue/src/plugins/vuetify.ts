/**
 * plugins/vuetify.ts
 *
 * Framework documentation: https://vuetifyjs.com`
 */

// Styles
import "@mdi/font/css/materialdesignicons.css";
import "vuetify/styles";
import { aliases, mdi } from "vuetify/iconsets/mdi-svg";

// Composables
import { createVuetify, type ThemeDefinition } from "vuetify";

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
  theme: {
    defaultTheme: "shuttleDark",
    themes: {
      shuttleDark: darkTheme,
      shuttleLight: lightTheme,
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
