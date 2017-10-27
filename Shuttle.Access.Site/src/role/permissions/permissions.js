import Component from 'can-component/';
import DefineList from 'can-define/list/';
import DefineMap from 'can-define/map/';
import view from './permissions.stache!';
import resources from '~/resources';
import Permissions from '~/permissions';
import Api from '~/api';
import each from 'can-util/js/each/';
import makeArray from 'can-util/js/make-array/';
import router from '~/router';
import alerts from '~/alerts';
import localisation from '~/localisation';
import state from '~/state';

var setPermission = new Api('roles/setpermission');
var permissions = new Api('permissions');
var permissionStatus = new Api('roles/permissionstatus');

resources.add('role', { action: 'permissions', permission: Permissions.Manage.RolePermissions });

const RolePermission = DefineMap.extend(
    'role-permission',
    {
        seal: false
    },
    {
        permission: 'string',
        active: 'boolean',
        working: 'boolean',

        toggle: function() {
            var self = this;

            if (this.working) {
                alerts.show({ message: localisation.value('workingMessage'), name: 'working-message' });
                return;
            }

            this.active = !this.active;
            this.working = true;

            setPermission.post({
                    roleId: router.data.id,
                    permission: this.permission,
                    active: this.active
                })
                .then(function() {
                    self.working = false;
                })
                .catch(function() {
                    self.working = false;
                });
        },

        rowClass: {
            get: function() {
                return this.active ? 'text-success success' : 'text-muted';
            }
        }
    }
);

var rolePermissions = new Api({
    endpoint: 'roles/{id}/permissions',
    Map: RolePermission
});

export const ViewModel = DefineMap.extend(
    'role-permission',
    {
        isResolved: { type: 'boolean', value: false },

        columns: {
            value: [
                {
                    columnTitle: 'active',
                    columnClass: 'col-md-1',
                    columnType: 'view',
                    view:
                        '<span ($click)="toggle()" class="glyphicon {{#if active}}glyphicon-check{{else}}glyphicon-unchecked{{/if}}" /><span class="glyphicon {{#if working}}glyphicon-hourglass{{/if}}" />'
                },
                {
                    columnTitle: 'role:permission',
                    attributeName: 'permission'
                }
            ]
        },

        permissions: {
            value: new DefineList()
        },

        init: function() {
            var self = this;

            this.refresh();

            this.on('workingCount',
                function() {
                    self.getPermissionStatus();
                });

            state.title = localisation.value('role:permissions.title');

            state.controls.push({
                type: 'back-button'
            });

            state.controls.push({
                type: 'refresh-button',
                click: 'refresh',
                context: this
            });
        },

        add: function() {
        },

        refresh: function() {
            'use strict';
            var self = this;

            this.isResolved = false;

            self.permissions.replace(new DefineList());

            permissions.list()
                .then(function(availablePermissionsResponse) {
                    var availablePermissions = makeArray(availablePermissionsResponse);

                    rolePermissions.list({ id: router.data.id })
                        .then(function(rolePermissions) {
                            each(availablePermissions,
                                function(availablePermission) {
                                    const permission = availablePermission.permission;
                                    const active = rolePermissions.filter(function(item) {
                                            return item.permission === permission;
                                        }).length >
                                        0;

                                    self.permissions.push(new RolePermission({
                                        permission: permission,
                                        active: active
                                    }));
                                });
                        })
                        .then(function() {
                            self.isResolved = true;
                        });
                });
        },

        getPermissionItem: function(permission) {
            var result;

            each(this.permissions,
                function(item) {
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
                return this.permissions.filter(function(item) {
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

        getPermissionStatus: function() {
            var self = this;

            if (this.workingCount === 0) {
                return;
            }

            var data = {
                roleId: router.data.id,
                permissions: []
            };

            each(this.workingItems,
                function(item) {
                    data.permissions.push(item.permission);
                });

            permissionStatus.post(data)
                .then(function(response) {
                    each(response.data,
                        function(item) {
                            const permissionItem = self.getPermissionItem(item.permission);

                            if (!permissionItem) {
                                return;
                            }

                            permissionItem.working = !(permissionItem.active === item.active);
                        });
                })
                .then(function() {
                    setTimeout(self.getPermissionStatus(), 1000);
                });
        }
    }
);

export default Component.extend({
    tag: 'sentinel-role-permissions',
    ViewModel,
    view
});