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

resources.add('queue', { action: 'list', permission: Permissions.Manage.Queues });

var queues = new Api('queues/{id}');

export const ViewModel = DefineMap.extend({
    columns: { Value: DefineList },
    refreshTimestamp: { type: 'string' },

    get queuesPromise() {
        const refreshTimestamp = this.refreshTimestamp;
        return queues.list();
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
                columnTitle: 'queue:queue-uri',
                attributeName: 'uri'
            });

            columns.push({
                columnTitle: 'remove',
                columnClass: 'col-md-1',
                columnType: 'remove-button',
                buttonContext: this,
                buttonClick: 'remove'
            });
        }

        state.title = localisation.value('queue:list.title');

        state.controls.push({
            type: 'button',
            title: 'add',
            click: 'add',
            elementClass: 'btn-primary',
            context: this,
            permission: 'sentinel://queue/add'
        });

        state.controls.push({
            type: 'refresh-button',
            click: 'refresh',
            context: this
        });
    },

    add: function() {
        router.goto('queue/add');
    },

    refresh: function() {
        this.refreshTimestamp = Date.now();
    },

    remove: function(row) {
        queues.delete({ id: row.id })
            .then(function() {
                alerts.show({ message: localisation.value('itemRemovalRequested', { itemName: localisation.value('queue:title') }), name: 'item-removal' });
            });
    },

    clone: function(row) {
        state.push('queue', row);

        this.add();
    }
});


export default Component.extend({
    tag: 'sentinel-queue-list',
    ViewModel,
    view
});