import { RouteRecordRaw, createRouter, createWebHistory } from "vue-router";
import { useSessionStore } from "@/stores/session";
import { useAlertStore } from "@/stores/alert";

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
    path: "/dashboard",
    name: "dashboard",
    component: () => import("../views/Dashboard.vue"),
  },
  {
    path: "/permissions",
    name: "permissions",
    component: () => import("../views/Permissions.vue"),
    meta: {
      permission: "access://permission/view",
    },
  },
  {
    path: "/permission",
    name: "permission",
    component: () => import("../views/Permission.vue"),
    meta: {
      permission: "access://permission/manage",
    },
  },
  {
    path: "/permission/:id/rename",
    name: "permission-rename",
    component: () => import("../views/PermissionRename.vue"),
    meta: {
      permission: "access://permission/manage",
    },
  },
  {
    path: "/roles",
    name: "roles",
    component: () => import("../views/Roles.vue"),
    meta: {
      permission: "access://role/view",
    },
  },
  {
    path: "/role",
    name: "role",
    component: () => import("../views/Role.vue"),
    meta: {
      permission: "access://role/manage",
    },
  },
  {
    path: "/role/:id/rename",
    name: "role-rename",
    component: () => import("../views/RoleRename.vue"),
    meta: {
      permission: "access://role/manage",
    },
  },
  {
    path: "/roles/:id/permissions",
    name: "role-permissions",
    component: () => import("../views/RolePermissions.vue"),
    meta: {
      permission: "access://role/view",
    },
  },
  {
    path: "/identities",
    name: "identities",
    component: () => import("../views/Identities.vue"),
    meta: {
      permission: "access://identity/view",
    },
  },
  {
    path: "/identity",
    name: "identity",
    component: () => import("../views/Identity.vue"),
    meta: {
      permission: "access://identity/manage",
    },
  },
  {
    path: "/identity/:id/rename",
    name: "identity-rename",
    component: () => import("../views/IdentityRename.vue"),
    meta: {
      permission: "access://identity/manage",
    },
  },
  {
    path: "/password/:id",
    name: "password",
    component: () => import("../views/Password.vue"),
  },
];
//     {
//       path: "/identities/:id/roles",
//       name: "identity-roles",
//       component: () => import("../views/IdentityRoles.vue"),
//       meta: {
//         permission: "access://identity/view",
//       },
//     },
//   ];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
});

router.beforeEach(async (to) => {
  const sessionStore = useSessionStore();

  if (!sessionStore.initialized) {
    return;
  }

  if (!!to.meta.permission && !sessionStore.hasPermission(to.meta.permission as string)) {
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

export default router;
