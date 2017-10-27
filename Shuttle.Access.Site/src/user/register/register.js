import Component from 'can-component/';
import DefineMap from 'can-define/map/';
import view from './register.stache!';
import resources from '~/resources';
import Permissions from '~/permissions';
import Api from '~/api';
import router from '~/router';
import security from '~/security';
import validator from 'can-define-validate-validatejs';

resources.add('user', { action: 'register', permission: Permissions.Manage.Users });

var users = new Api('users');

export const ViewModel = DefineMap.extend(
    'user-register',
    {
        username: {
            type: 'string',
            validate: {
                presence: true
            }
        },
        password: {
            type: 'string',
            validate: {
                presence: true
            }
        },
        working: {
            type: 'boolean',
            value: false
        },
        title: {
            get: function() {
                return security.isUserRequired ? 'user:register.user-required' : 'user:register.title';
            }
        },
        showClose: {
            get: function() {
                return !security.isUserRequired;
            }
        },

        hasErrors: function() {
            return !!this.errors();
        },

        register: function() {
            var self = this;

            if (this.hasErrors()) {
                return false;
            }

            this.working = true;

            users.post({
                username: this.username,
                password: this.password
            })
                .then(function() {
                    if (security.isUserRequired) {
                        security.isUserRequired = false;

                        router.goto('dashboard');
                    } else {
                        router.goto('user/list');
                    }
                })
                .then(function() {
                    self.working = false;
                });

            return false;
        },

        close: function() {
            router.goto('user/list');
        }
    }
);

validator(ViewModel);

export default Component.extend({
    tag: 'sentinel-user-register',
    ViewModel,
    view,
    events: {
        'inserted': function() {
            $('#username').focus();
        }
    }
});