import Permissions from '~/permissions';

var map = [
    {
        href: '#!user/list',
        text: 'user:list.title',
        permission: Permissions.View.Users
    },
    {
        href: '#!role/list',
        text: 'role:list.title',
        permission: Permissions.View.Roles
    }
];

export default map;