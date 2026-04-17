import type { NavigationItem } from "@/access";
import Permissions from "../permissions";
import {
  mdiAccount,
  mdiAccountGroup,
  mdiBadgeAccount,
  mdiDomain,
  mdiShield,
  mdiViewDashboard,
} from "@mdi/js";

const map: NavigationItem[] = [
  {
    to: "/dashboard",
    title: "dashboard",
    icon: mdiViewDashboard,
  },
  {
    to: "/identities",
    title: "identities",
    permission: Permissions.Identities.View,
    icon: mdiAccount,
  },
  {
    to: "/roles",
    title: "roles",
    permission: Permissions.Roles.View,
    icon: mdiAccountGroup,
  },
  {
    to: "/permissions",
    title: "permissions",
    permission: Permissions.Permissions.View,
    icon: mdiShield,
  },
  {
    to: "/sessions",
    title: "sessions",
    permission: Permissions.Sessions.View,
    icon: mdiBadgeAccount,
  },
  {
    to: "/tenants",
    title: "tenants",
    permission: Permissions.Tenants.View,
    icon: mdiDomain,
  },
];

export default map;
