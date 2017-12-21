import Component from 'can-component/';
import DefineList from 'can-define/list/';
import DefineMap from 'can-define/map/';
import view from './permissions.stache!';
import resources from '~/resources';
import Permissions from '~/permissions';
import Api from 'shuttle-can-api';
import each from 'can-util/js/each/';
import makeArray from 'can-util/js/make-array/';
import router from '~/router';
import localisation from '~/localisation';
import state from '~/state';

var api = {
    role: new Api('roles/{id}'),
    setPermission: new Api('roles/setpermission'),
    permissions: new Api('permissions'),
    permissionStatus: new Api('roles/permissionstatus')
}

resources.add('role', {action: 'permissions', permission: Permissions.Manage.RolePermissions});

const RolePermission = DefineMap.extend({
    permission: 'string',
    active: 'boolean',
    working: 'boolean',

    toggle: function () {
        var self = this;

        if (this.working) {
            state.alerts.show({message: localisation.value('workingMessage'), name: 'working-message'});
            return;
        }

        this.active = !this.active;
        this.working = true;

        api.setPermission.post({
            roleId: router.data.id,
            permission: this.permission,
            active: this.active
        })
            .then(function () {
                self.working = false;
            })
            .catch(function () {
                self.working = false;
            });
    },

    rowClass: {
        get: function () {
            return this.active ? 'text-success success' : 'text-muted';
        }
    }
});

export const ViewModel = DefineMap.extend({
    role: {
        Type: DefineMap
    },

    isResolved: {type: 'boolean', value: false},

    columns: {
        value: [
            {
                columnTitle: 'active',
                columnClass: 'col-1',
                stache: '<cs-checkbox click:from="@toggle" checked:bind="active" checkedClass:from="\'fa-toggle-on\'" uncheckedClass:from="\'fa-toggle-off\'"/>{{#if working}}<i class="fa fa-hourglass-o" aria-hidden="true"></i>{{/if}}'
            },
            {
                columnTitle: 'role:permission',
                columnClass: 'col',
                attributeName: 'permission'
            }
        ]
    },

    permissions: {
        value: new DefineList()
    },

    init: function () {
        var self = this;

        this.refresh();

        this.on('workingCount',
            function () {
                self.getPermissionStatus();
            });

        state.title = 'role:permissions.title';
        state.navbar.addButton({
            type: 'back'
        });
        state.navbar.addButton({
            type: 'refresh',
            viewModel: this
        });
    },

    refresh: function () {
        'use strict';
        var self = this;

        this.isResolved = false;

        self.permissions.replace(new DefineList());

        api.permissions.list()
            .then(function (availablePermissionsResponse) {
                var availablePermissions = makeArray(availablePermissionsResponse);

                api.role.item({id: router.data.id})
                    .then(function (response) {
                        self.role = response.data;

                        each(availablePermissions,
                            function (availablePermission) {
                                const permission = availablePermission.permission;
                                const active = self.role.permissions.filter(function (item) {
                                        return item.permission === permission;
                                    }).length >
                                    0;

                                self.permissions.push(new RolePermission({
                                    permission: permission,
                                    active: active
                                }));
                            });
                    })
                    .then(function () {
                        self.isResolved = true;
                    });
            });
    },

    getPermissionItem: function (permission) {
        var result;

        each(this.permissions,
            function (item) {
                if (result) {
                    return;
                }

                if (item.permission === permission) {
                    result = item;
                }
            });

        return result;
    },

    workingItems: {
        get() {
            return this.permissions.filter(function (item) {
                return item.working;
            });
        }
    },

    workingCount: {
        type: 'number',
        get() {
            return this.workingItems.length;
        }
    },

    getPermissionStatus: function () {
        var self = this;

        if (this.workingCount === 0) {
            return;
        }

        var data = {
            roleId: router.data.id,
            permissions: []
        };

        each(this.workingItems,
            function (item) {
                data.permissions.push(item.permission);
            });

        api.permissionStatus.post(data)
            .then(function (response) {
                each(response.data,
                    function (item) {
                        const permissionItem = self.getPermissionItem(item.permission);

                        if (!permissionItem) {
                            return;
                        }

                        permissionItem.working = !(permissionItem.active === item.active);
                    });
            })
            .then(function () {
                setTimeout(self.getPermissionStatus(), 1000);
            });
    }
});

export default Component.extend({
    tag: 'access-role-permissions',
    ViewModel,
    view
});