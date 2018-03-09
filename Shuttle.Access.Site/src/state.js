import DefineMap from 'can-define/map/';
import DefineList from 'can-define/list/';
import guard from 'shuttle-guard';
import route from 'can-route';
import {alerts} from 'shuttle-canstrap/alerts/';
import loader from '@loader';
import stache from 'can-stache';
import localisation from '~/localisation';

export const NavbarControlMap = DefineMap.extend({
    stache: {
        type: 'string',
        get: function (value) {
            if (!value) {
                throw new Error('The \'NavbarControlMap\' does not have a \'view\' specified.')
            }

            return value;
        }
    },
    view: {
        type: 'string',
        get() {
            return stache(this.stache)(this.viewModel);
        }
    },
    viewModel: {
        type: '*'
    }
});

export const NavbarControlList = DefineList.extend({
    '#': NavbarControlMap,
    clear() {
        this.replace([]);
    }
});

export const Navbar = DefineMap.extend({
    controls: {
        Default: NavbarControlList
    },
    addButton(options) {
        guard.againstUndefined(options, 'options')
        guard.againstUndefined(options.type, 'options.type')

        var permission = options.permission || '';
        var type = options.type.toLowerCase();
        var click = options.click;

        if (options.type !== 'back' && !click) {
            switch (type) {
                case 'add':
                case 'refresh':
                case 'remove': {
                    click = type;
                    break;
                }
                default: {
                    throw new Error('No \'click\' method name has been specified.')
                }
            }
        }

        switch (type) {
            case 'back': {
                this.controls.push({
                    stache: '<cs-button-back text:from="\'back\'" elementClass:from="\'btn-sm mr-2\'"/>'
                });
                break;
            }
            case 'refresh': {
                this.controls.push({
                    viewModel: options.viewModel,
                    stache: `<cs-button-refresh click:from="@${click}" elementClass:from="'btn-sm mr-2'"/>`
                });
                break;
            }
            case 'add': {
                this.controls.push({
                    viewModel: options.viewModel,
                    stache: `<cs-button click:from="@${click}" elementClass:from="\'btn-sm mr-2\'" permission:from="\'${permission}\'" text:from="\'${options.text || 'add'}\'"/>`
                });
                break;
            }
            case 'remove': {
                this.controls.push({
                    viewModel: options.viewModel,
                    stache: `<cs-button-remove click:from="@${click}" elementClass:from="\'btn-sm mr-2\'" permission:from="\'${permission}\'"/>`
                });
                break;
            }
            default: {
                throw new Error(`Unhandled button type '${options.type}'.`);
            }
        }
    }
});

var State = DefineMap.extend({
    route: route,
    alerts: {
        get() {
            return alerts;
        }
    },
    debug: {
        type: 'boolean',
        get() {
            return loader.debug;
        }
    },
    navbar: {
        Default: Navbar
    },
    title: {
        type: 'string',
        default: '',
        get(value) {
            return localisation.value(value);
        }
    },
    modal: {
        Type: DefineMap.extend({
            confirmation: {
                Type: DefineMap.extend({
                    primaryClick: {
                        type: '*'
                    },
                    message: {
                        type: 'string',
                        default: ''
                    },
                    show(options) {
                        guard.againstUndefined(options, "options");

                        this.message = options.message || 'No \'message\' passed in the confirmation options.';
                        this.primaryClick = options.primaryClick;

                        $('#modal-confirmation').modal({show: true});
                    }
                }),
                default() {
                    return {};
                }
            }
        }),
        default() {
            return {};
        }
    },
});

export default new State();