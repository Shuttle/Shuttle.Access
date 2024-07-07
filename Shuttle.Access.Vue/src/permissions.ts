const Permissions = {
  Register: {
    IdentityRequired: "access://identity/required",
    Role: "access://roles/register",
    Identity: "access://identity/register",
    Permission: "access://permission/register",
  },
  Remove: {
    Role: "access://role/remove",
    Identity: "access://identity/remove",
    Permission: "access://permission/remove",
  },
  View: {
    Dashboard: "access://dashboard/view",
    Role: "access://role/view",
    Identity: "access://identity/view",
    Permission: "access://permission/view",
    Sessions: "access://sessions/view",
  },
  Activate: {
    Identity: "access://identity/activate",
  },
};

export default Permissions;
