import type { NavigationItem } from "@/access";
import Permissions from "../permissions";

const map: NavigationItem[] = [
  {
    to: "/dashboard",
    title: "dashboard",
  },
  {
    to: "/identities",
    title: "identities",
    permission: Permissions.Identities.View,
  },
  {
    to: "/roles",
    title: "roles",
    permission: Permissions.Roles.View,
  },
  {
    to: "/permissions",
    title: "permissions",
    permission: Permissions.Permissions.View,
  },
  {
    to: "/sessions",
    title: "sessions",
    permission: Permissions.Sessions.View,
  },
];

export default map;
