import Component from 'can-component/';
import DefineList from 'can-define/list/';
import DefineMap from 'can-define/map/';
import view from './roles.stache!';
import resources from '~/resources';
import Permissions from '~/permissions';
import Api from '~/api';
import each from 'can-util/js/each/';
import makeArray from 'can-util/js/make-array/';
import router from '~/router';
import localisation from '~/localisation';
import alerts from '~/alerts';
import state from '~/state';

resources.add('user', { action: 'roles', permission: Permissions.Manage.Users });

const UserRole = DefineMap.extend(
    'user-role',
    {
        seal: false
    },
    {
        roleName: 'string',
        active: 'boolean',

        toggle: function() {
            var self = this;

            if (this.working) {
                alerts.show({ message: localisation.value('workingMessage'), name: 'working-message' });
                return;
            }

            this.active = !this.active;
            this.working = true;

            setRole.post({
                    userId: router.data.id,
                    roleName: this.roleName,
                    active: this.active
                })
                .then(function(response) {
                    self.working = false;

                    if (response.success) {
                        return;
                    }

                    switch (response.failureReason.toLowerCase()) {
                    case 'lastadministrator':
                    {
                        self.active = true;
                        self.working = false;

                        alerts.show({
                            message: localisation.value('user:exceptions.last-administrator'),
                            name: 'last-administrator',
                            type: 'danger'
                        });

                        break;
                    }
                    }
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

var roles = new Api({
    endpoint: 'roles',
    Map: UserRole
});

var userRoles = new Api('users/{id}/roles');
var setRole = new Api('users/setrole');

export const ViewModel = DefineMap.extend(
    'user-role',
    {
        isResolved: { type: 'boolean', value: false },

        init: function() {
            var self = this;

            this.refresh();

            this.on('workingCount',
                function() {
                    self.getRoleStatus();
                });

            state.title = localisation.value('user:list.roles');

            state.controls.push({
                type: 'back-button'
            });

            state.controls.push({
                type: 'refresh-button',
                click: 'refresh',
                context: this
            });
        },

        refresh() {
            const self = this;

            this.isResolved = false;

            self.roles.replace(new DefineList());

            roles.list()
                .then(function(availableRoles) {
                    availableRoles = makeArray(availableRoles);
                    availableRoles.push({ id: '', roleName: 'administrator' });

                    userRoles.list({ id: router.data.id })
                        .then(function(userRoles) {
                            each(availableRoles,
                                function(availableRole) {
                                    const active = userRoles.filter(function(item) {
                                            return item.roleName === availableRole.roleName;
                                        }).length >
                                        0;
                                    const roleName = availableRole.roleName;

                                    self.roles.push(new UserRole({
                                        roleName: roleName,
                                        active: active
                                    }));
                                });
                        })
                        .then(function() {
                            self.isResolved = true;
                        });
                });
        },

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
                    columnTitle: 'user:roleName',
                    attributeName: 'roleName'
                }
            ]
        },

        roles: {
            value: new DefineList()
        },

        getRoleItem: function(roleName) {
            var result;

            each(this.roles,
                function(item) {
                    if (result) {
                        return;
                    }

                    if (item.roleName === roleName) {
                        result = item;
                    }
                });

            return result;
        },

        workingItems: {
            get() {
                return this.roles.filter(function(item) {
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

        getRoleStatus: function() {
            var self = this;

            if (this.workingCount === 0) {
                return;
            }

            var data = {
                userId: router.data.id,
                roles: []
            };

            each(this.workingItems,
                function(item) {
                    data.roles.push(item.roleName);
                });

            api.post('users/rolestatus', { data: data })
                .done(function(response) {
                    each(response.data,
                        function(item) {
                            const roleItem = self.getRoleItem(item.roleName);

                            if (!roleItem) {
                                return;
                            }

                            roleItem.working = !(roleItem.active === item.active);
                        });
                })
                .always(function() {
                    setTimeout(self.getRoleStatus(), 1000);
                });
        }
    });

export default Component.extend({
    tag: 'sentinel-user-roles',
    ViewModel,
    view
});