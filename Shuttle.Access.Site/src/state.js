import DefineMap from 'can-define/map/';
import DefineList from 'can-define/list/';
import guard from 'shuttle-guard';
import route from 'can-route';
import { alerts } from 'shuttle-canstrap/alerts/';
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
        Value: DefineMap.extend({
            confirmation: {
                Value: DefineMap.extend({
                    primaryClick: 'observable',
                    message: 'string'
                })
            }
        })
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