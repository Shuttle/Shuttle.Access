import Component from 'can-component/';
import DefineMap from 'can-define/map/';
import DefineList from 'can-define/list/';
import view from './list.stache!';
import resources from '~/resources';
import Permissions from '~/permissions';
import router from '~/router';
import Api from 'shuttle-can-api';
import localisation from '~/localisation';
import state from '~/state';

resources.add('role', {action: 'list', permission: Permissions.Manage.Roles});

const Map = DefineMap.extend({
    id: 'string',
    rolwName: 'string',

    remove: function () {
        users.delete({id: this.id})
            .then(function () {
                state.alerts.show({
                    message: localisation.value('itemRemovalRequested',
                        {itemName: localisation.value('role:role')})
                });
            });
    },

    permissions: function () {
        state.data.put('role', this);

        router.goto({
            resource: 'role',
            action: 'permissions',
            id: this.id
        });
    }
});

var roles = new Api({
    endpoint: 'roles/{id}',
    Map
});

export const ViewModel = DefineMap.extend(
    'role-list',
    {
        columns: {Value: DefineList},
        refreshTimestamp: {type: 'string'},

        get rolesPromise() {
            const refreshTimestamp = this.refreshTimestamp;
            return roles.list();
        },

        init: function () {
            const columns = this.columns;

            if (!columns.length) {
                columns.push({
                    columnTitle: 'role:permissions.title',
                    columnClass: 'col-1',
                    stache: '<cs-button text:from="\'role:permissions.title\'" click:from="@permissions" elementClass:from="\'btn-sm\'"/>'
                });

                columns.push({
                    columnTitle: 'role:name',
                    columnClass: 'col',
                    attributeName: 'roleName'
                });

                columns.push({
                    columnTitle: 'remove',
                    columnClass: 'col-1',
                    stache: '<cs-button-remove click:from="@remove" elementClass:from="\'btn-sm\'"/>'
                });
            }

            state.title = 'role:list.title';

            state.navbar.addButton({
                type: 'add',
                viewModel: this,
                permission: 'access://user/register'
            });

            state.navbar.addButton({
                type: 'refresh',
                viewModel: this
            });
        },

        add: function () {
            router.goto({
                resource: 'role',
                action: 'add'
            });
        },

        refresh: function () {
            this.refreshTimestamp = Date.now();
        }
    });


export default Component.extend({
    tag: 'access-role-list',
    ViewModel,
    view
});