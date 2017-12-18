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

const Map = DefineMap.extend(
    'user',
    {
        seal: false
    },
    {
        id: 'string',
        username: 'string',
        dateRegistered: 'date',
        registeredBy: 'string'
    });

var users = new Api({
    endpoint: 'users/{id}',
    Map: Map
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
                view: '<cs-button text:from="\'user:list.roles\'" click:from="@roles" elementClass:from="\'btn-sm\'"/>'
            });

            columns.push({
                columnTitle: 'user:username',
                attributeName: 'username'
            });

            columns.push({
                columnTitle: 'user:dateRegistered',
                attributeName: 'dateRegistered'
            });

            columns.push({
                columnTitle: 'user:registeredBy',
                attributeName: 'registeredBy'
            });

            columns.push({
                columnTitle: 'remove',
                view: '<cs-button-remove click:from="@remove" elementClass:from="\'btn-sm\'"/>'
            });
        }

        state.title = localisation.value('user:list.title');

        state.navbarControls.push({
            context: this,
            view: '<cs-button permission:from="\'access://user/register\'" click:from="@add" text:from="\'add\'" elementClass:from="\'btn-primary btn-sm mr-2\'"/>'
        });

        state.navbarControls.push({
            context: this,
            view: '<cs-button-refresh click:from="@refresh" elementClass:from="\'btn-sm mr-2\'"/>'
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
    },

    remove: function (user) {
        users.delete({id: user.id})
            .then(function () {
                state.alerts.show({
                    message: localisation.value('itemRemovalRequested',
                        {itemName: localisation.value('user:title')})
                });
            });
    },

    roles: function (user) {
        router.goto({
            resource: 'user',
            ation: 'roles',
            id: user.id
        });
    }
});

export default Component.extend({
    tag: 'access-user-list',
    ViewModel,
    view
});