import DefineMap from 'can-define/map/';
import DefineList from 'can-define/list/';
import guard from 'shuttle-guard';
import route from 'can-route';
import {alerts} from 'shuttle-canstrap/alerts/';
import loader from '@loader';
import stache from "can-stache";

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
    addBackButton() {
        this.controls.push({
            stache: '<cs-button-back text:from="\'back\'" elementClass:from="\'btn-sm mr-2\'"/>'
        })
    },
    addRemoveButton(options) {
        guard.againstUndefined(options, 'options')

        if (!options.click) {
            throw new Error('No \'click\' method name has been specified.')
        }

        this.controls.push({
            viewModel: options.viewModel,
            stache: `<cs-button-remove click:from="@${options.click}" elementClass:from="\'btn-sm mr-2\'"/>`
        })
    },
    addRefreshButton(options) {
        guard.againstUndefined(options, 'options')

        if (!options.click) {
            throw new Error('No \'click\' method name has been specified.')
        }

        this.controls.push({
            viewModel: options.viewModel,
            stache: `<cs-button-refresh click:from="@${options.click}" elementClass:from="'btn-sm mr-2'"/>`
        })
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
        Value: DefineList
    },
    navbar: {
        Type: Navbar,
        value: {}
    },
    title: {
        type: 'string',
        value: ''
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
    push: function (name, value) {
        guard.againstUndefined(name, 'name');

        this.data.push({name: name, value: value});
    },

    pop: function (name) {
        guard.againstUndefined(name, 'name');

        let result;
        let removeIndex = -1;

        this.data.forEach(function (item, index) {
            if (item.name === name) {
                result = item.value;
                removeIndex = index;

                return false;
            } else {
                return true;
            }
        });

        if (removeIndex > -1) {
            this.data.splice(removeIndex, 1);
        }

        return result;
    }
});

export default new State();