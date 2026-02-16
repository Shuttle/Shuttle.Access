<template>
  <v-navigation-drawer v-model="drawerStore.showNavigationDrawer" :permanent="!$vuetify.display.mobile" class="pt-2">
    <div class="flex justify-end">
      <v-btn :icon="mdiArrowCollapseLeft"
        @click.stop="drawerStore.showNavigationDrawer = !drawerStore.showNavigationDrawer" class="mr-4" flat></v-btn>

    </div>
    <v-list>
      <v-list-item v-for="(item, i) in items" :key="i" :value="item" color="primary" :to="item.to">
        <template v-slot:prepend>
          <v-icon :icon="item.icon"></v-icon>
        </template>
        <v-list-item-title>{{ item.title }}</v-list-item-title>
      </v-list-item>
    </v-list>
  </v-navigation-drawer>
  <v-app-bar class="shadow-sm">
    <template v-slot:prepend v-if="sessionStore.isAuthenticated">
      <v-app-bar-nav-icon variant="text"
        @click.stop="drawerStore.showNavigationDrawer = !drawerStore.showNavigationDrawer"></v-app-bar-nav-icon>
    </template>
    <v-app-bar-title class="cursor-pointer font-bold"
      @click="$router.push('/dashboard')">Shuttle.Access</v-app-bar-title>
    <template v-slot:append>
      <div class="flex items-center">
        <v-switch class="mr-2" v-model="isDarkTheme" :false-icon="mdiWhiteBalanceSunny" :true-icon="mdiWeatherNight"
          hide-details />
        <v-btn v-if="!sessionStore.isAuthenticated" :icon="mdiLogin" @click.prevent="signIn"></v-btn>
        <v-btn v-else :icon="mdiDotsVertical" variant="text"
          @click.stop="drawerStore.showProfileDrawer = !drawerStore.showProfileDrawer"></v-btn>
      </div>
    </template>
  </v-app-bar>
  <v-navigation-drawer v-model="drawerStore.showProfileDrawer" location="right" temporary>
    <v-list>
      <v-list-item :title="sessionStore.identityName" class="select-none"></v-list-item>
      <v-divider></v-divider>
      <v-list-item :prepend-icon="mdiShieldAccountOutline" to="/password/token"
        :title="t('change-password')"></v-list-item>
      <v-list-item :prepend-icon="mdiLogout" @click.prevent="signOut" :title="t('sign-out')"></v-list-item>
    </v-list>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import map from "./navigation-map";
import { mdiArrowCollapseLeft, mdiDotsVertical, mdiLogin, mdiLogout, mdiWhiteBalanceSunny, mdiWeatherNight, mdiShieldAccountOutline } from '@mdi/js';
import { computed, ref, watch } from "vue";
import { useSessionStore } from "@/stores/session";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { useTheme } from 'vuetify';
import type { NavigationItem } from "@/access";
import { useAlertStore } from "@/stores/alert";
import configuration from "@/configuration";
import axios from "axios";
import { useDrawerStore } from "@/stores/drawer";

const { t } = useI18n({ useScope: 'global' });

const drawerStore = useDrawerStore();
const sessionStore = useSessionStore();
const router = useRouter();
const theme = useTheme();
const alertStore = useAlertStore();

const storedTheme = localStorage.getItem('app-theme') || theme.global.name.value;
const isDarkTheme: Ref<boolean> = ref(storedTheme === 'dark');

const applyTheme = (selectedTheme: string) => {
  if (selectedTheme === "dark") {
    document.documentElement.classList.add("dark");
  } else {
    document.documentElement.classList.remove("dark");
  }

  theme.change(selectedTheme)
}

applyTheme(isDarkTheme.value ? 'dark' : 'light');

watch(isDarkTheme, (newValue) => {
  const selectedTheme = newValue ? 'dark' : 'light'
  applyTheme(selectedTheme);
  localStorage.setItem('app-theme', selectedTheme)
})

const items = computed(() => {
  const result: any[] = [];

  map.forEach((item: NavigationItem) => {
    if (!item.permission || sessionStore.hasPermission(item.permission)) {
      result.push({
        icon: item.icon,
        title: t(item.title),
        to: item.to || ""
      });
    }

    return result.length ? result : [{ title: t("sign-in"), props: { to: "/signin" } }];
  });

  return result;
});

const signIn = () => {
  router.push({ name: 'sign-in' })
}

const signOut = () => {
  axios.delete("v1/sessions/self", {
    baseURL: configuration.getUrl(),
    headers: {
      "Authorization": `Shuttle.Access token=${sessionStore.token}`
    }
  })
    .then(() => {
      sessionStore.signOut()
      drawerStore.showNavigationDrawer = false;
      drawerStore.showProfileDrawer = false;

      signIn();
    })
    .catch((error) => {
      alertStore.add({
        message: error.toString(),
        type: "error",
        name: "sign-out-exception"
      });
    });
}

</script>
