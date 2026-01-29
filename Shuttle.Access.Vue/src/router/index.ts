import type { RouteRecordRaw } from "vue-router";
import { createRouter, createWebHistory } from "vue-router";
import { useSessionStore } from "@/stores/session";
import { useAlertStore } from "@/stores/alert";
import Permissions from "../permissions";
import { i18n } from "@/i18n";
import Dashboard from "../views/Dashboard.vue";

const routes: Array<RouteRecordRaw> = [
  {
    path: "",
    component: Dashboard,
  },
  {
    path: "/",
    component: Dashboard,
  },
  {
    path: "/dashboard",
    name: "dashboard",
    component: Dashboard,
  },
  {
    path: "/identities",
    name: "identities",
    component: () => import("../views/Identities.vue"),
    meta: {
      permission: Permissions.Identities.View,
    },
    children: [
      {
        path: "identity",
        name: "identity",
        component: () => import("../views/Identity.vue"),
        meta: {
          permission: Permissions.Identities.Manage,
        },
      },
      {
        path: "password/:id",
        name: "identity-password",
        props: true,
        component: () => import("../views/Password.vue"),
      },
      {
        path: "identity/:id/description",
        name: "identity-description",
        component: () => import("../views/IdentityDescription.vue"),
        props: true,
        meta: {
          permission: Permissions.Identities.Manage,
        },
      },
      {
        path: "identity/:id/rename",
        name: "identity-rename",
        component: () => import("../views/IdentityRename.vue"),
        props: true,
        meta: {
          permission: Permissions.Identities.Manage,
        },
      },
      {
        path: "/identities/:id/roles",
        name: "identity-roles",
        component: () => import("../views/IdentityRoles.vue"),
        meta: {
          permission: Permissions.Identities.View,
        },
      },
    ],
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
    path: "/permissions",
    name: "permissions",
    component: () => import("../views/Permissions.vue"),
    meta: {
      permission: Permissions.Permissions.View,
    },
    children: [
      {
        path: "permission",
        name: "permission",
        component: () => import("../views/Permission.vue"),
        meta: {
          permission: Permissions.Permissions.Manage,
        },
      },
      {
        path: "permission/json",
        name: "permission-json",
        component: () => import("../views/PermissionJson.vue"),
        meta: {
          permission: Permissions.Permissions.Manage,
        },
      },
      {
        path: ":id/rename",
        name: "permission-rename",
        component: () => import("../views/PermissionRename.vue"),
        props: true,
        meta: {
          permission: Permissions.Permissions.Manage,
        },
      },
      {
        path: "permission/:id/permission",
        name: "permission-description",
        component: () => import("../views/PermissionDescription.vue"),
        props: true,
        meta: {
          permission: Permissions.Identities.Manage,
        },
      },
      {
        path: "permission/upload",
        name: "permission-upload",
        component: () => import("../views/PermissionUpload.vue"),
        meta: {
          permission: Permissions.Permissions.Manage,
        },
      },
    ],
  },
  {
    path: "/roles",
    name: "roles",
    component: () => import("../views/Roles.vue"),
    meta: {
      permission: Permissions.Roles.View,
    },
    children: [
      {
        path: "role",
        name: "role",
        component: () => import("../views/Role.vue"),
        meta: {
          permission: Permissions.Roles.Manage,
        },
      },
      {
        path: "role/json",
        name: "role-json",
        component: () => import("../views/RoleJson.vue"),
        meta: {
          permission: Permissions.Roles.Manage,
        },
      },
      {
        path: "role/:id/rename",
        name: "role-rename",
        component: () => import("../views/RoleRename.vue"),
        props: true,
        meta: {
          permission: Permissions.Roles.Manage,
        },
      },
      {
        path: "roles/:id/permissions",
        name: "role-permissions",
        props: true,
        component: () => import("../views/RolePermissions.vue"),
        meta: {
          permission: Permissions.Roles.View,
        },
      },
      {
        path: "role/upload",
        name: "role-upload",
        component: () => import("../views/RoleUpload.vue"),
        meta: {
          permission: Permissions.Roles.Manage,
        },
      },
    ],
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
    path: "/tenants",
    name: "tenants",
    component: () => import("../views/Tenants.vue"),
    meta: {
      permission: Permissions.Tenants.View,
    },
    children: [
      {
        path: "tenant",
        name: "tenant",
        component: () => import("../views/Tenant.vue"),
        meta: {
          permission: Permissions.Tenants.Manage,
        },
      },
    ],
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

  if (!sessionStore.isInitialized) {
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
    !sessionStore.isAuthenticated &&
    to.name !== "signin"
  ) {
    return { name: "signin" };
  }
});

export default router;
