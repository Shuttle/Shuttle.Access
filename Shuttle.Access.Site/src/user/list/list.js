import Component from 'can-component/';
import DefineMap from 'can-define/map/';
import view from './list.stache!';
import resources from '~/resources';
import Permissions from '~/permissions';
import router from '~/router';
import Api from 'shuttle-can-api';
import localisation from '~/localisation';
import state from '~/state';
import {ColumnList} from 'shuttle-canstrap/table/';

resources.add('user', {action: 'list', permission: Permissions.View.Users});

const Map = DefineMap.extend({
    id: 'string',
    username: 'string',
    dateRegistered: 'date',
    registeredBy: 'string',

    remove: function () {
        users.delete({id: this.id})
            .then(function () {
                state.alerts.show({
                    message: localisation.value('itemRemovalRequested',
                        {itemName: localisation.value('user:title')})
                });
            });
    },

    roles: function () {
        router.goto({
            resource: 'user',
            action: 'roles',
            id: this.id
        });
    }
});

var users = new Api({
    endpoint: 'users/{id}',
    Map
});

export const ViewModel = DefineMap.extend({
    columns: {
        Type: ColumnList,
        value: []
    },

    refreshTimestamp: {
        type: 'string'
    },

    get usersPromise() {
        const refreshTimestamp = this.refreshTimestamp;
        return users.list();
    },

    init: function () {
        const columns = this.columns;

        if (!columns.length) {
            columns.push({
                columnTitle: 'user:list.roles',
                columnClass: 'col-1',
                stache: '<cs-button text:from="\'user:list.roles\'" click:from="@roles" elementClass:from="\'btn-sm\'"/>'
            });

            columns.push({
                columnTitle: 'user:username',
                columnClass: 'col',
                attributeName: 'username'
            });

            columns.push({
                columnTitle: 'user:dateRegistered',
                columnClass: 'col-3',
                attributeName: 'dateRegistered'
            });

            columns.push({
                columnTitle: 'user:registeredBy',
                columnClass: 'col',
                attributeName: 'registeredBy'
            });

            columns.push({
                columnTitle: 'remove',
                columnClass: 'col-1',
                stache: '<cs-button-remove click:from="@remove" elementClass:from="\'btn-sm\'"/>'
            });
        }

        state.title = 'user:list.title';

        state.navbar.addButton({
            type: 'add',
            viewModel: this,
            permission: 'access://user/register',
            click: 'add'
        });

        state.navbar.addButton({
            type: 'refresh',
            viewModel: this,
            click: 'refresh'
        });
    },

    add: function () {
        router.goto({
            resource: 'user',
            action: 'register'
        });
    },

    refresh: function () {
        this.refreshTimestamp = Date.now();
    }
});

export default Component.extend({
    tag: 'access-user-list',
    ViewModel,
    view
});