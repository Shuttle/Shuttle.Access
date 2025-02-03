import type { RouteRecordRaw } from "vue-router";
import { createRouter, createWebHistory } from "vue-router";
import { useSessionStore } from "@/stores/session";
import { useAlertStore } from "@/stores/alert";
import Permissions from "../permissions";
import { useBreadcrumbStore } from "@/stores/breadcrumb";
import type { Breadcrumb } from "@/access";
import { i18n } from "@/i18n";

const ignoreBreadcrumbs = ["sign-in", "oauth"];

const routes: Array<RouteRecordRaw> = [
  {
    path: "/dashboard",
    name: "dashboard",
    component: () => import("../views/Dashboard.vue"),
  },
  {
    path: "/identities",
    name: "identities",
    component: () => import("../views/Identities.vue"),
    meta: {
      permission: Permissions.Identities.View,
    },
  },
  {
    path: "/identity",
    name: "identity",
    component: () => import("../views/Identity.vue"),
    meta: {
      permission: Permissions.Identities.Manage,
    },
  },
  {
    path: "/identity/:id/rename",
    name: "identity-rename",
    component: () => import("../views/IdentityRename.vue"),
    props: true,
    meta: {
      permission: Permissions.Identities.Manage,
    },
  },
  {
    path: "/oauth",
    name: "oauth",
    component: () => import("../views/OAuth.vue"),
  },
  {
    path: "/password/:id",
    name: "password",
    props: true,
    component: () => import("../views/Password.vue"),
  },
  {
    path: "/identities/:id/roles",
    name: "identity-roles",
    component: () => import("../views/IdentityRoles.vue"),
    meta: {
      permission: Permissions.Identities.View,
    },
  },
  {
    path: "/permissions",
    name: "permissions",
    component: () => import("../views/Permissions.vue"),
    meta: {
      permission: Permissions.Permissions.View,
    },
  },
  {
    path: "/permission",
    name: "permission",
    component: () => import("../views/Permission.vue"),
    meta: {
      permission: Permissions.Permissions.Manage,
    },
  },
  {
    path: "/permission/:id/rename",
    name: "permission-rename",
    component: () => import("../views/PermissionRename.vue"),
    props: true,
    meta: {
      permission: Permissions.Permissions.Manage,
    },
  },
  {
    path: "/roles",
    name: "roles",
    component: () => import("../views/Roles.vue"),
    meta: {
      permission: Permissions.Roles.View,
    },
  },
  {
    path: "/role",
    name: "role",
    component: () => import("../views/Role.vue"),
    meta: {
      permission: Permissions.Roles.Manage,
    },
  },
  {
    path: "/role/:id/rename",
    name: "role-rename",
    component: () => import("../views/RoleRename.vue"),
    props: true,
    meta: {
      permission: Permissions.Roles.Manage,
    },
  },
  {
    path: "/roles/:id/permissions",
    name: "role-permissions",
    props: true,
    component: () => import("../views/RolePermissions.vue"),
    meta: {
      permission: Permissions.Roles.View,
    },
  },
  {
    path: "/sessions",
    name: "sessions",
    component: () => import("../views/Sessions.vue"),
    meta: {
      permission: Permissions.Sessions.View,
    },
  },
  {
    path: "/signin/:applicationName?",
    name: "sign-in",
    props: true,
    component: () => import("../views/SignIn.vue"),
  },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
});

router.beforeEach(async (to) => {
  const sessionStore = useSessionStore();

  if (!sessionStore.initialized) {
    return;
  }

  if (
    !!to.meta.permission &&
    !sessionStore.hasPermission(to.meta.permission as string)
  ) {
    useAlertStore().add({
      message: i18n.global.t("exceptions.insufficient-permission"),
      type: "info",
      name: "insufficient-permission",
    });

    return false;
  }

  if (
    !!to.meta.authenticated &&
    !sessionStore.authenticated &&
    to.name !== "signin"
  ) {
    return { name: "signin" };
  }
});

router.afterEach(async (to) => {
  const breadcrumbStore = useBreadcrumbStore();

  var name = typeof to.name === "string" ? to.name : undefined;

  if (!name || ignoreBreadcrumbs.includes(name)) {
    breadcrumbStore.clear();
    return;
  }

  if (name === "dashboard") {
    breadcrumbStore.clear();
  }

  const existingIndex = breadcrumbStore.breadcrumbs.findIndex(
    (route: Breadcrumb) => route.path === to.path
  );

  if (existingIndex === -1) {
    breadcrumbStore.addBreadcrumb({
      name: to.name,
      path: to.path,
    });
  } else {
    breadcrumbStore.removeBreadcrumbsAfter(existingIndex);
  }
});

export default router;
