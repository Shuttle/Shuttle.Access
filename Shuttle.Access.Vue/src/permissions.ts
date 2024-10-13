const Permissions = {
    Dashboard: {
        View: "access://dashboard/view",
    },
    Identities: {
        Activate: "access://identities/activate",
        Manage: "access://identities/manage",
        Register: "access://identities/register",
        Remove: "access://identities/remove",
        View: "access://identities/view"
    },
    Permissions: {
        Manage: "access://permissions/manage",
        Register: "access://permissions/register",
        Remove: "access://permissions/remove",
        View: "access://permissions/view",
    },
    Roles: {
        Manage: "access://roles/manage",
        Register: "access://roles/register",
        Remove: "access://roles/remove",
        View: "access://roles/view",
    },
    Sessions: {
        Remove: "access://sessions/remove",
        View: "access://sessions/view",
    }
};

export default Permissions;
