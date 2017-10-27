import $ from 'jquery';
import DefineMap from 'can-define/map/';
import DefineList from 'can-define/list/';
import Api from '~/api';
import localisation from '~/localisation';
import alerts from '~/alerts';
import each from 'can-util/js/each/';

var anonymous = new Api('anonymouspermissions');
var sessions = new Api('sessions');

var Security = DefineMap.extend({
    username: { type: 'string', value: '' },
    token: { type: 'string', value: undefined },
    isUserRequired: 'boolean',

    permissions: {
        value: new DefineList()
    },

    hasSession: function() {
        return this.token != undefined;
    },

    hasPermission: function(permission) {
        var result = false;
        var permissionCompare = permission.toLowerCase();

        this.permissions.each(function(item) {
            if (result) {
                return;
            }

            result = item.permission === '*' || item.permission.toLowerCase() === permissionCompare;
        });

        return result;
    },

    removePermission: function(permission) {
        this.permissions = this.permissions.filter(function(item) {
            return item.permission !== permission;
        });
    },

    start: function() {
        var self = this;

        return anonymous.list()
            .then(function(data) {
                const username = localStorage.getItem('username');
                const token = localStorage.getItem('token');

                self.isUserRequired = data.isUserRequired;

                each(data.permissions,
                    function(item) {
                        self._addPermission('anonymous', item.permission);
                    });

                if (!!username && !!token) {
                    return self.login({ username: username, token: token })
                        .then(function(response) {
                            return response;
                        });
                }

                return data;
            })
            .catch(function() {
                alerts.show({ message: localisation.value('exceptions.anonymous-permissions'), type: 'danger' });
            });
    },

    _addPermission: function(type, permission) {
        this.permissions.push({ type: type, permission: permission });
    },

    login: function(options) {
        var self = this;

        if (!options) {
            return $.Deferred().reject();
        }

        var usingToken = !!options.token;

        return sessions.post({
            username: options.username,
            password: options.password,
            token: options.token
        })
            .then(function(response) {
                if (response.registered) {
                    localStorage.setItem('username', options.username);
                    localStorage.setItem('token', response.token);

                    self.username = options.username;
                    self.token = response.token;
                    self.isUserRequired = false;

                    alerts.remove({ name: 'login-failure' });

                    self.removeUserPermissions();

                    each(response.permissions,
                        function(permission) {
                            self._addPermission('user', permission);
                        });
                } else {
                    if (usingToken) {
                        self.username = undefined;
                        self.token = undefined;

                        localStorage.removeItem('username');
                        localStorage.removeItem('token');
                    } else {
                        alerts.show({
                            message: localisation.value('exceptions.login', { username: options.username }),
                            type: 'danger',
                            name: 'login-failure'
                        });
                    }
                }
            })
            .catch(function(error) {
                alerts.show(error, 'danger');
            });
    },

    logout: function() {
        this.username = undefined;
        this.token = undefined;

        localStorage.removeItem('username');
        localStorage.removeItem('token');

        this.removeUserPermissions();
    },

    removeUserPermissions: function() {
        this.permissions = this.permissions.filter(function(item) {
            return item.type !== 'user';
        });
    },

    loginStatus: {
        get: function() {
            return this.isUserRequired ? 'user-required' : this.token == undefined ? 'not-logged-in' : 'logged-in';
        }
    }
});

var security = new Security();

$.ajaxPrefilter(function(options, originalOptions) {
    options.beforeSend = function(xhr) {
        if (security.token) {
            xhr.setRequestHeader('sentinel-sessiontoken', security.token);
        }

        if (originalOptions.beforeSend) {
            originalOptions.beforeSend(xhr);
        }
    };
});

export default security;