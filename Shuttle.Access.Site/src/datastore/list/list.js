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

resources.add('datastore', { action: 'list', permission: Permissions.Manage.DataStores });

var datastores = new Api('datastores/{id}');

export const ViewModel = DefineMap.extend({
    columns: { Value: DefineList },
    refreshTimestamp: { type: 'string' },

    get list () {
        const refreshTimestamp = this.refreshTimestamp;
        return datastores.list();
    },

    init: function() {
        const columns = this.columns;

        if (!columns.length) {
            columns.push({
                columnTitle: 'clone',
                columnClass: 'col-md-1',
                columnType: 'button',
                buttonTitle: 'clone',
                buttonClick: 'clone',
                buttonContext: this
            });

            columns.push({
                columnTitle: 'name',
                attributeName: 'name'
            });

            columns.push({
                columnTitle: 'datastore:connection-string',
                attributeName: 'connectionString'
            });

            columns.push({
                columnTitle: 'datastore:provider-name',
                attributeName: 'providerName'
            });

            columns.push({
                columnTitle: 'remove',
                columnClass: 'col-md-1',
                columnType: 'remove-button',
                buttonContext: this,
                buttonClick: 'remove'
            });
        }

        state.title = localisation.value('datastore:list.title');

        state.controls.push({
            type: 'button',
            title: 'add',
            click: 'add',
            elementClass: 'btn-primary',
            context: this,
            permission: 'sentinel://datastore/add'
        });

        state.controls.push({
            type: 'refresh-button',
            click: 'refresh',
            context: this
        });
    },

    add: function() {
        router.goto('datastore/add');
    },

    refresh: function() {
        this.refreshTimestamp = Date.now();
    },

    remove: function(row) {
        datastores.delete({ id: row.id })
            .then(function() {
                alerts.show({ message: localisation.value('itemRemovalRequested', { itemName: localisation.value('datastore:title') }), name: 'item-removal' });
            });
    },

    clone: function(row) {
        state.push('datastore', row);

        this.add();
    }
});

export default Component.extend({
    tag: 'sentinel-datastore-list',
    ViewModel,
    view
});