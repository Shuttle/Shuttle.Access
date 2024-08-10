<template>
    <v-app-bar :elevation="2">
        <template v-slot:prepend>
            <v-app-bar-nav-icon variant="text" @click.stop="drawer = !drawer"></v-app-bar-nav-icon>
        </template>
        <v-app-bar-title>Shuttle.Access</v-app-bar-title>
        <template v-slot:append>
            <v-btn :icon="mdiLogin" v-if="!sessionStore.authenticated" @click.prevent="signIn"></v-btn>
            <v-btn :icon="mdiLogout" v-if="sessionStore.authenticated" @click.prevent="signOut"></v-btn>
        </template>
    </v-app-bar>
    <v-navigation-drawer v-model="drawer" :location="$vuetify.display.mobile ? 'bottom' : undefined" temporary>
        <v-list @click="$router.push({ path: item.route })" :items="items"></v-list>
    </v-navigation-drawer>
    <!-- <header>
        <div
            class="px-4 border-0 border-b border-solid border-gray-200 dark:border-gray-700 fixed top-0 left-0 w-full bg-[color:var(--color-background)] z-50">
            <Navbar>
                <template #start>
                    <div class="flex flex-row items-center mr-2">
                        <div class="font-bold text-orange-500 mt-[2px]">Shuttle.Access</div>
                    </div>
                </template>
                <template #navigation>
                    <Navigation :items="items" @click="route" />
                </template>
                <template #end>
                    <div class="flex flex-row items-center">
                        <div class="mx-2">
                            <Toggle aria-label="toggle dark mode" v-model="darkMode" :onIcon="MoonIcon"
                                :offIcon="SunIcon" @click="toggleAppearance" />
                        </div>
                        <div class="hidden sm:block">
                            <Navigation :items="profileItems" dropdown-alignment="right" @click="route" />
                        </div>
                    </div>
                </template>
                <template #navigation-minimal>
                    <Navigation :minimal="true" :items="items" @click="route" />
                </template>
                <template #bottom-minimal class="sm:hidden">
                    <Navigation :minimal="true" :items="profileItems" dropdown-alignment="right" @click="route" />
                </template>
            </Navbar>
        </div>
    </header> -->
</template>

<script setup>
import map from "./navigation-map";
import { mdiLogin, mdiLogout } from '@mdi/js';
import { computed, ref } from "vue";
import { useSessionStore } from "@/stores/session";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";

const { t } = useI18n({ useScope: 'global' });

const drawer = ref(false);

const sessionStore = useSessionStore();
const router = useRouter();

const items = computed(() => {
    var result = [];

    map.forEach((item) => {
        var add = false;
        var subitems = [];

        if (!item.permission || sessionStore.hasPermission(item.permission)) {
            if (item.items !== undefined) {
                item.items.forEach((subitem) => {
                    if (
                        !subitem.permission ||
                        sessionStore.hasPermission(subitem.permission)
                    ) {
                        add = true;

                        subitems.push({
                            title: t(subitem.text),
                            props: {
                                to: subitem.to
                            }
                        });
                    }
                });
            } else {
                add = true;
            }

            if (add) {
                const navItem = {
                    title: t(item.text),
                    props: {
                        to: item.to || ""
                    }
                }

                if (subitems.length) {
                    navItem.children = subitems;
                }

                result.push(navItem);
            }
        }
    });

    return result.length ? result : [{ title: t("sign-in"), props: { to: "/signin" } }];
});

const profileItems = computed(() => {
    return sessionStore.authenticated ?
        [{
            buttonIcon: UserIcon,
            items: [
                {
                    text: t("password"),
                    to: "/password/token"
                },
                {
                    type: "divider"
                },
                {
                    text: t("sign-out"),
                    click: () => {
                        sessionStore.signOut();
                        router.push({ name: "sign-in" })
                    }
                }
            ]
        }] : [
            {
                text: t("sign-in"),
                to: "/signin"
            }
        ]
});

const signIn = () => {
    router.push({ name: 'sign-in' })
}

const signOut = () => {
    sessionStore.signOut();

    signIn();
}

const route = (item) => {
    router.push({ path: item.to });
}

const storageKey = 'shuttle-theme-appearance';
let userPreference = localStorage?.getItem(storageKey) || 'auto'
const query = window.matchMedia(`(prefers-color-scheme: dark)`)
const classList = document.documentElement.classList
let isDark = userPreference === 'auto' ? query.matches : userPreference === 'dark'
const setClass = (dark) => classList[dark ? 'add' : 'remove']('dark')

query.onchange = (e) => {
    if (userPreference === 'auto') {
        setClass((isDark = e.matches));
    }
}

const toggleAppearance = () => {
    if (typeof localStorage === 'undefined') {
        return;
    }

    setClass((isDark = !isDark));

    localStorage.setItem(
        storageKey,
        (userPreference = isDark
            ? query.matches
                ? 'auto'
                : 'dark'
            : query.matches
                ? 'light'
                : 'auto')
    );
}

const darkMode = ref(isDark);

setClass(isDark);

</script>