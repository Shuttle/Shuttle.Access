import DefineMap from 'can-define/map/';
import DefineList from 'can-define/list/';
import guard from 'shuttle-guard';
import route from 'can-route';
import {alerts} from 'shuttle-canstrap/alerts/';
import loader from '@loader';

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
    navbarControls: {
        Value: DefineList
    },
    title: {
        type: 'string',
        value: '(title)'
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