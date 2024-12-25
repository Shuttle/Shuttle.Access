import type { RouteLocationNormalized, RouteRecordRaw } from "vue-router";
import { createRouter, createWebHistory } from "vue-router";
import { useSessionStore } from "@/stores/session";
import { useAlertStore } from "@/stores/alert";
import Permissions from "../permissions";
import { useBreadcrumbStore } from "@/stores/breadcrumb";
import type { Breadcrumb } from "@/access";

export const messages = {
  insufficientPermission:
    "You do not have permission to access the requested view.  Please contact your system administrator if you require access.",
};

const routes: Array<RouteRecordRaw> = [
  {
    path: "/signin",
    name: "sign-in",
    component: () => import("../views/SignIn.vue"),
  },
  {
    path: "/oauth/:",
    name: "oauth",
    component: () => import("../views/OAuth.vue"),
  },
  {
    path: "/dashboard",
    name: "dashboard",
    component: () => import("../views/Dashboard.vue"),
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
      message: messages.insufficientPermission,
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
