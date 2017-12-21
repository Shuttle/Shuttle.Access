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
        Type: NavbarControlList,
        value: []
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

var Data = DefineMap.extend({
    items: {
        Type: DefineList,
        value: []
    },
    put: function (name, value) {
        guard.againstUndefined(name, 'name');

        this.remove(name);
        this.items.push({name: name, value: value});
    },
    pop: function (name) {
        guard.againstUndefined(name, 'name');

        let result;
        let removeIndex = -1;

        this.items.forEach(function (item, index) {
            if (item.name === name) {
                result = item.value;
                removeIndex = index;

                return false;
            }

            return true;
        });

        if (removeIndex > -1) {
            this.splice(removeIndex, 1);
        }

        return result;
    },
    get: function (name) {
        guard.againstUndefined(name, 'name');

        let result;

        this.items.forEach(function (item, index) {
            if (item.name === name) {
                result = item.value;

                return false;
            }

            return true;
        });

        return result;
    },
    remove: function (name) {
        guard.againstUndefined(name, 'name');

        let removeIndex = -1;

        this.items.forEach(function (item, index) {
            if (item.name === name) {
                removeIndex = index;

                return false;
            }

            return true;
        });

        if (removeIndex > -1) {
            this.splice(removeIndex, 1);
        }
    }
});

var State = DefineMap.extend({
    route: route,
    alerts: {
        value: alerts
    },
    debug: {
        type: 'boolean',
        value: loader.debug
    },
    data: {
        Type: Data,
        value: {}
    },
    navbar: {
        Type: Navbar,
        value: {}
    },
    title: {
        type: 'string',
        value: '',
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
                        value: ''
                    },
                    show(options) {
                        guard.againstUndefined(options, "options");

                        this.message = options.message || 'No \'message\' passed in the confirmation options.';
                        this.primaryClick = options.primaryClick;

                        $('#modal-confirmation').modal({show: true});
                    }
                }),
                value: {}
            }
        }),
        value: {}
    },
});

export default new State();