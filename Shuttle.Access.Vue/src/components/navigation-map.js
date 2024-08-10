import Permissions from '../permissions';

var map = [
    {
        to: '/dashboard',
        text: 'dashboard',
        permission: Permissions.View.Dashboard
    },
    {
        to: '/identities',
        text: 'identities',
        permission: Permissions.View.Identity
    },
    {
        to: '/roles',
        text: 'roles',
        permission: Permissions.View.Role
    },
    {
        to: '/permissions',
        text: 'permissions',
        permission: Permissions.View.Permission
    },
];

export default map;