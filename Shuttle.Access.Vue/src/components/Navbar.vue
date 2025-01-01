<template>
  <v-app-bar :elevation="2">
    <template v-slot:prepend>
      <v-app-bar-nav-icon variant="text" @click.stop="showMainDrawer = !showMainDrawer"></v-app-bar-nav-icon>
    </template>
    <v-app-bar-title class="cursor-pointer" @click="$router.push('/dashboard')">Shuttle.Access</v-app-bar-title>
    <template v-slot:append>
      <div class="flex items-center">
        <v-switch class="mr-2" v-model="isDarkTheme" :false-icon="mdiWhiteBalanceSunny" :true-icon="mdiWeatherNight"
          hide-details />
        <v-btn v-if="!sessionStore.authenticated" :icon="mdiLogin" @click.prevent="signIn"></v-btn>
        <v-btn v-else :icon="mdiDotsVertical" variant="text"
          @click.stop="showProfileDrawer = !showProfileDrawer"></v-btn>
      </div>
    </template>
  </v-app-bar>
  <v-navigation-drawer v-model="showMainDrawer" :location="$vuetify.display.mobile ? 'bottom' : undefined" temporary>
    <v-list :items="items"></v-list>
  </v-navigation-drawer>
  <v-navigation-drawer v-model="showProfileDrawer" location="right" temporary>
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
import { mdiDotsVertical, mdiLogin, mdiLogout, mdiWhiteBalanceSunny, mdiWeatherNight, mdiShieldAccountOutline } from '@mdi/js';
import { computed, ref, watch } from "vue";
import { useSessionStore } from "@/stores/session";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { useTheme } from 'vuetify';
import type { NavigationItem } from "@/access";

const { t } = useI18n({ useScope: 'global' });

const showMainDrawer = ref(false);
const showProfileDrawer = ref(false);

const sessionStore = useSessionStore();
const router = useRouter();
const theme = useTheme();

const storedTheme = localStorage.getItem('app-theme') || theme.global.name.value;
const isDarkTheme: Ref<boolean> = ref(storedTheme === 'shuttleDark');

theme.global.name.value = isDarkTheme.value ? 'shuttleDark' : 'shuttleLight';

watch(isDarkTheme, (newValue) => {
  const selectedTheme = newValue ? 'shuttleDark' : 'shuttleLight';
  theme.global.name.value = selectedTheme;
  localStorage.setItem('app-theme', selectedTheme);
});

const items = computed(() => {
  const result: any[] = [];

  map.forEach((item: NavigationItem) => {
    var add = false;

    if (!item.permission || sessionStore.hasPermission(item.permission)) {
      result.push({
        title: t(item.title),
        props: {
          to: item.to || ""
        }
      });
    }

    return result.length ? result : [{ title: t("sign-in"), props: { to: "/signin" } }];
  });

  return result;
});

// const profileItems = computed(() => {
//   return sessionStore.authenticated ?
//     [{
//       buttonIcon: UserIcon,
//       items: [
//         {
//           text: t("password"),
//           to: "/password/token"
//         },
//         {
//           type: "divider"
//         },
//         {
//           text: t("sign-out"),
//           click: () => {
//             sessionStore.signOut();
//             router.push({ name: "sign-in" })
//           }
//         }
//       ]
//     }] : [
//       {
//         text: t("sign-in"),
//         to: "/signin"
//       }
//     ]
// });

const signIn = () => {
  router.push({ name: 'sign-in' })
}

const signOut = () => {
  sessionStore.signOut();

  signIn();
}

</script>
