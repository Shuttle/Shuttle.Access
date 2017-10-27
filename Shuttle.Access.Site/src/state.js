import DefineMap from 'can-define/map/';
import DefineList from 'can-define/list/';
import guard from '~/guard';
import route from 'can-route';
import loader from '@loader';

var State = DefineMap.extend({
    route: route,
    debug: { type: 'boolean', value: loader.debug },
    data: { Value: DefineList },
    controls: { Value: DefineList },
    title: { type: 'string', value: 'Page Title' },

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

    push: function(name, value) {
        guard.againstUndefined(name, 'name');

        this.data.push({name: name, value: value});
    },

    pop: function(name) {
        guard.againstUndefined(name, 'name');
        
        let result;
        let removeIndex = -1;

        this.data.forEach(function(item, index) {
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