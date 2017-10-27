import Component from 'can-component/';
import DefineMap from 'can-define/map/';
import DefineList from 'can-define/list/';
import view from './manage.stache!';
import resources from '~/resources';
import Permissions from '~/permissions';
import alerts from '~/alerts';
import localisation from '~/localisation';
import Api from '~/api';
import each from 'can-util/js/each/';
import $ from 'jquery';
import state from '~/state';

resources.add('message', { action: 'manage', permission: Permissions.Manage.Messages });

const Message = DefineMap.extend(
    'message',
    {
        seal: false
    },
    {
        checked: {
            type: 'boolean',
            value: false
        },

        selected: {
            type: 'boolean',
            value: false
        },

        toggleCheck: function(ev) {
            this.checked = !this.checked;

            ev.stopPropagation();
        },

        messageSelected: function(message) {
            this.viewModel.messageSelected(message);
        }
    }
);

var messages = new Api({
    endpoint: 'messages',
    Map: Message
});

var fetchMessages = new Api('messages/fetch');
var transferMessages = new Api('messages/transfer');
export const ViewModel = DefineMap.extend(
    'manage',
    {
        message: {},
        messageRows: {},
        messages: { Value: DefineList },
        hasMessages: { type: 'boolean', value: false },
        showMessages: { type: 'boolean', value: true },
        sourceQueueUri: { type: 'string', value: '' },
        destinationQueueUri: { type: 'string', value: '' },
        fetching: { type: 'boolean', value: false },
        fetchCount: { type: 'number', value: 5 },
        refreshTimestamp: { type: 'string' },
        messageActions: { value: new DefineList() },

        showTitle () {
            state.title = localisation.value('message:' + (this.showMessages ? 'title-manage' : 'message'));
        },

        get messagesPromise() {
            const self = this;
            const refreshTimestamp = this.refreshTimestamp;

            return messages.list()
                .then(function(list) {
                    self.messages = list;
                    self.hasMessages = list.length > 0;

                    return list;
                });
        },

        columns: {
            value: new DefineList()
        },

        messageColumns: {
            value: [
                {
                    columnClass: 'col-md-2',
                    columnTitle: 'name',
                    attributeName: 'name'
                },
                {
                    columnTitle: 'value',
                    attributeName: 'value'
                }
            ]
        },

        hasSourceQueueUri: {
            get: function() {
                return !!this.sourceQueueUri;
            }
        },

        hasDestinationQueueUri: {
            get: function() {
                return !!this.destinationQueueUri;
            }
        },

        noCheckedMessages: {
            get: function() {
                var checked = false;

                each(this.messages, function(item) {
                    if (checked) {
                        return;
                    }

                    checked = checked || item.checked;
                });

                return !checked;
            }
        },

        init: function() {
            var self = this;
            let columns = this.columns;
            let messageActions = this.messageActions;

            if (!columns.length) {
                columns.push({
                    checked: false,
                    columnClass: 'col-md-1',
                    columnTitle: 'check',
                    columnType: 'view',
                    view: '<span ($click)="toggleCheck(%event)" class="glyphicon {{#if checked}}glyphicon-check{{else}}glyphicon-unchecked{{/if}}" />'
                });

                columns.push(
                {
                    columnClass: 'col-md-2',
                    columnTitle: 'message:message-id',
                    attributeName: 'messageId'
                });

                columns.push(
                {
                    columnTitle: 'message:message',
                    columnType: 'view',
                    view: '<pre>{{message}}</pre>'
                });
            }

            if (!messageActions.length) {
                messageActions.push({
                    text: "message:return-to-source",
                    click: function() {
                        self._transfer('ReturnToSourceQueue');
                    }
                });

                messageActions.push({
                    text: "message:send-to-recipient",
                    click: function() {
                        self._transfer('SendToRecipientQueue');
                    }
                });

                messageActions.push({
                    text: "message:stop-ignoring",
                    click: function() {
                        self._transfer('StopIgnoring');
                    }
                });

                messageActions.push({
                    text: "remove",
                    click: function() {
                        self._transfer('Remove');
                    }
                });
            }

            this.showTitle();
            this.refresh();
        },

        refresh: function() {
            this.refreshTimestamp = Date.now();
        },

        fetch: function() {
            var self = this;

            if (!this.hasSourceQueueUri) {
                alerts.show({ message: localisation.value('message:exceptions.source-queue-uri'), name: 'message:exceptions.source-queue-uri', type: 'danger' });

                return false;
            }

            this.fetching = true;

            fetchMessages.post({
                    queueUri: this.sourceQueueUri,
                    count: this.fetchCount || 1
                })
                .then(function(response) {
                    alerts.show({ message: localisation.value('message:count-retrieved', { count: response.data.countRetrieved }), name: 'message:count-retrieved'});

                    self.refresh();

                    self.fetching = false;
                })
                .catch(function() {
                    self.fetching = false;
                });

            return true;
        },

        checkedMessageIds: function() {
            return $.map(this.messages, function(message) { return message.checked ? message.messageId : undefined; });
        },

        move: function() {
            return this._transfer('Move');
        },

        copy: function() {
            return this._transfer('Copy');
        },

        returnToSourceQueue: function() {
            return this._transfer('Copy');
        },

        _transfer: function(action) {
            const self = this;

            if (!this.destinationQueueUri && (action === 'Move' || action === 'Copy')) {
                alerts.show({ message: localisation.value('message:exceptions.destination-queue-uri'), name: 'message:exceptions.destination-queue-uri', type: 'danger' });

                return false;
            }

            transferMessages.post({
                    messageIds: this.checkedMessageIds(),
                    destinationQueueUri: this.destinationQueueUri,
                    action: action
                })
                .then(function(response) {
                    self.refresh();

                    return response;
                });

            return true;
        },

        closeMessageView () {
            this.showMessages = true;
            this.showTitle();
        },

        messageSelected: function(message) {
            var self = this;

            this.message = message;
            this.messageRows = new DefineList();

            this.addMessageRow('MessageId', message.messageId);
            this.addMessageRow('SourceQueueUri', message.sourceQueueUri);
            this.addMessageRow('AssemblyQualifiedName', message.assemblyQualifiedName);
            this.addMessageRow('CompressionAlgorithm', message.compressionAlgorithm);
            this.addMessageRow('CorrelationId', message.correlationId);
            this.addMessageRow('EncryptionAlgorithm', message.encryptionAlgorithm);
            this.addMessageRow('ExpiryDate', message.expiryDate);

            if (message.failureMessages && message.failureMessages.length) {
                each(message.failureMessages, function(item, index) {
                    self.addMessageRow('FailureMessages.' + index, item);
                });
            } else {
                this.addMessageRow('FailureMessages', "(none)");
            }

            if (message.headers && message.headers.length) {
                each(message.headers, function(item, index) {
                    self.addMessageRow('Headers.' + index, item);
                });
            } else {
                this.addMessageRow('Headers', "(none)");
            }

            this.addMessageRow('IgnoreTillDate', message.ignoreTillDate);
            this.addMessageRow('MessageReceivedId', message.messageReceivedId);
            this.addMessageRow('MessageType', message.messageType);
            this.addMessageRow('PrincipalIdentityName', message.principalIdentityName);
            this.addMessageRow('RecipientInboxWorkQueueUri', message.recipientInboxWorkQueueUri);
            this.addMessageRow('SendDate', message.sendDate);
            this.addMessageRow('SenderInboxWorkQueueUri', message.senderInboxWorkQueueUri);

            this.showMessages = false;
            this.showTitle();
        },

        addMessageRow: function(name, value) {
            this.messageRows.push({ name: name, value: value });
        },

        checkAll: function() {
            this._setCheckMarks(true);
        },

        checkNone: function() {
            this._setCheckMarks(false);
        },

        checkInvert: function() {
            this._setCheckMarks();
        },

        _setCheckMarks: function(value) {
            each(this.messages, function(item) {
                item.checked = (value == undefined? !item.checked: value);
            });
        }
    });


export default Component.extend({
    tag: 'sentinel-message-manage',
    ViewModel,
    view
});