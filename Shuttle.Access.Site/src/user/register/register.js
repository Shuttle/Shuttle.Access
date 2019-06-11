import {DefineMap,Component} from 'can';
import view from './register.stache!';
import resources from '~/resources';
import Permissions from '~/permissions';
import Api from 'shuttle-can-api';
import router from '~/router';
import access from 'shuttle-access';
import validator from 'can-define-validate-validatejs';

resources.add('user', {action: 'register', permission: Permissions.Manage.Users});

var users = new Api({ endpoint: 'users' });

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
            default: false
        },
        title: {
            get: function () {
                return access.isUserRequired ? 'user:register.user-required' : 'user:register.title';
            }
        },
        showClose: {
            get: function () {
                return !access.isUserRequired;
            }
        },

        hasErrors: function () {
            return !!this.errors();
        },

        register: function () {
            var self = this;

            if (this.hasErrors()) {
                return false;
            }

            this.working = true;

            users.post({
                username: this.username,
                password: this.password
            })
                .then(function () {
                    if (access.isUserRequired) {
                        access.isUserRequired = false;

                        router.goto({
                            resource: 'dashboard'
                        });
                    } else {
                        router.goto({
                            resource: 'user',
                            action: 'list'
                        });
                    }
                })
                .then(function () {
                    self.working = false;
                });

            return false;
        },

        close: function () {
            router.goto({
                resource: 'user',
                action: 'list'
            });
        }
    }
);

validator(ViewModel);

export default Component.extend({
    tag: 'access-user-register',
    ViewModel,
    view
});