import Component from 'can-component/';
import DefineMap from 'can-define/map/';
import DefineList from 'can-define/list/';
import view from './add.stache!';
import resources from '~/resources';
import Permissions from '~/permissions';
import router from '~/router';
import Api from '~/api';
import validator from 'can-define-validate-validatejs';
import state from '~/state';
import each from 'can-util/js/each/';
import localisation from '~/localisation';

resources.add('subscription', { action: 'add', permission: Permissions.Manage.Subscriptions });

var dataStores = new Api('datastores');
var subscriptions = new Api('subscriptions/{id}');

export const ViewModel = DefineMap.extend(
    'subscription',
    {
        dataStores: { Value: DefineList },

        init () {
            const self = this;

            state.title = localisation.value('subscription:list.title');

            self.dataStores.push({ value: undefined, label: 'select' });

            dataStores.list().then((response) => {
                each(response, (store) => {
                    self.dataStores.push({
                        value: store.id,
                        label: store.name
                    });
                });
            });

            const result = state.pop('subscription');

            if (!result) {
                return;
            }

            this.dataStoreId = result.dataStoreId;
            this.messageType = result.messageType;
            this.inboxWorkQueueUri = result.inboxWorkQueueUri;
        },

        dataStoreId: { 
            type: 'string',
            value: '',
            validate: {
                presence: true
            }
        },

        messageType: { 
            type: 'string',
            value: '',
            validate: {
                presence: true
            }
        },

        inboxWorkQueueUri: { 
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

            subscriptions.post({
                dataStoreId: this.dataStoreId,
                messageType: this.messageType,
                inboxWorkQueueUri: this.inboxWorkQueueUri
            });

            this.close();

            return false;
        },

        close: function() {
            router.goto('subscription/list');
        }
    }
);

validator(ViewModel);

export default Component.extend({
    tag: 'sentinel-subscription-add',
    ViewModel,
    view
});