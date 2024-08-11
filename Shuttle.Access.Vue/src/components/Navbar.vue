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
        <v-list :items="items"></v-list>
    </v-navigation-drawer>
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

const route = (item) => {
    router.push({ path: item.to });
}

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

</script>