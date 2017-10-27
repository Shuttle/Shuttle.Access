import Component from 'can-component/';
import DefineMap from 'can-define/map/';
import view from './send.stache!';
import resources from '~/resources';
import Permissions from '~/permissions';
import alerts from '~/alerts';
import localisation from '~/localisation';
import Api from '~/api';
import validator from 'can-define-validate-validatejs';
import state from '~/state';

resources.add('message', { action: 'send', permission: Permissions.Manage.Messages });

var messages = new Api('messages');

export const ViewModel = DefineMap.extend({
    destinationQueueUri: {
        type: 'string',
        value: '',
        validate: {
            presence: true
        }
    },

    message: {
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

    init() {
        state.title = localisation.value('message:title-send');
    },

    send () {
        const self = this;

        if (!!this.errors()) {
            return false;
        }

        messages.post({
            destinationQueueUri: this.destinationQueueUri,
            messageType: this.messageType,
            message: this.message
        })
        .then(function() {
                alerts.show({ message: localisation.value('message:sent') });
            });

        return false;
    }
});

validator(ViewModel);

export default Component.extend({
    tag: 'sentinel-message-send',
    ViewModel,
    view
});