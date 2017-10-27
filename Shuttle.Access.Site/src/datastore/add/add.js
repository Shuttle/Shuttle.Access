import Component from 'can-component/';
import DefineMap from 'can-define/map/';
import view from './add.stache!';
import resources from '~/resources';
import Permissions from '~/permissions';
import router from '~/router';
import Api from '~/api';
import validator from 'can-define-validate-validatejs';
import state from '~/state';
import localisation from '~/localisation';

resources.add('datastore', { action: 'add', permission: Permissions.Manage.DataStores });

var datastores = new Api('datastores/{id}');

export const ViewModel = DefineMap.extend(
    'queues',
    {
        init: function() {
            const result = state.pop('datastore');

            state.title = localisation.value('datastore:list.title');

            if (!result) {
                return;
            }

            this.name = result.name;
            this.connectionString = result.connectionString;
            this.providerName = result.providerName;
        },

        name: { 
            type: 'string',
            value: '',
            validate: {
                presence: true
            }
        },

        connectionString: { 
            type: 'string',
            value: '',
            validate: {
                presence: true
            }
        },

        providerName: { 
            type: 'string',
            value: '',
            validate: {
                presence: true
            }
        },

        add: function() {
            if (!!this.errors()) {
                return false;
            }

            datastores.post({
                name: this.name,
                connectionString: this.connectionString,
                providerName: this.providerName
            });

            this.close();

            return false;
        },

        close: function() {
            router.goto('datastore/list');
        }
    }
);

validator(ViewModel);

export default Component.extend({
    tag: 'sentinel-datastore-add',
    ViewModel,
    view
});