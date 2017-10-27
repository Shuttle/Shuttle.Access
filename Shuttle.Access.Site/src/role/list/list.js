import Component from 'can-component/';
import DefineMap from 'can-define/map/';
import DefineList from 'can-define/list/';
import view from './list.stache!';
import resources from '~/resources';
import Permissions from '~/permissions';
import router from '~/router';
import Api from '~/api';
import alerts from '~/alerts';
import localisation from '~/localisation';
import state from '~/state';

resources.add('role', { action: 'list', permission: Permissions.Manage.Roles });

var roles = new Api('roles/{id}');

export const ViewModel = DefineMap.extend(
    'role-list',
    {
        columns: { Value: DefineList },
        refreshTimestamp: { type: 'string' },

        get rolesPromise() {
            const refreshTimestamp = this.refreshTimestamp;
            return roles.list();
        },

        init: function() {
            const columns = this.columns;

            if (!columns.length) {
                columns.push( {
                    columnTitle: 'role:permissions.title',
                    columnClass: 'col-md-1',
                    columnType: 'button',
                    buttonTitle: 'role:permissions.title',
                    buttonClick: 'permissions',
                    buttonContext: this
                });

                columns.push({
                    columnTitle: 'role:name', 
                    attributeName: 'roleName'
                });

                columns.push({
                    columnTitle: 'remove', 
                    columnClass: 'col-md-1',
                    columnType: 'remove-button',
                    buttonContext: this,
                    buttonClick: 'remove'
                });
            }

            state.title = localisation.value('role:list.title');

            state.controls.push({
                type: 'button',
                title: 'add',
                click: 'add',
                elementClass: 'btn-primary',
                context: this,
                permission: 'sentinel://role/add'
            });

            state.controls.push({
                type: 'refresh-button',
                click: 'refresh',
                context: this
            });
        },

        add: function() {
            router.goto('role/add');
        },

        refresh: function() {
            this.refreshTimestamp = Date.now();
        },

        remove: function(row) {
            roles.delete({ id: row.id })
                .then(function() {
                    alerts.show({ message: localisation.value('itemRemovalRequested', { itemName: localisation.value('role:role') }) });
                });
        },

        permissions: function(row) {
            router.goto('role/' + row.id + '/permissions');
        }
    });


export default Component.extend({
    tag: 'sentinel-role-list',
    ViewModel,
    view
});