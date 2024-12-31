import Permissions from "../permissions";

var map = [
  {
    to: "/dashboard",
    text: "dashboard",
  },
  {
    to: "/identities",
    text: "identities",
    permission: Permissions.Identities.View,
  },
  {
    to: "/roles",
    text: "roles",
    permission: Permissions.Roles.View,
  },
  {
    to: "/permissions",
    text: "permissions",
    permission: Permissions.Permissions.View,
  },
];

export default map;
