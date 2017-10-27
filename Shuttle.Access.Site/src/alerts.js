import DefineMap from 'can-define/map/';
import DefineList from 'can-define/list/';
import stache from 'can-stache';
import each from 'can-util/js/each/';

var Alerts = DefineMap.extend({
    messages: { Value: DefineList },

    _key: { type: 'number', value: 1 },

    show: function(options) {
        if (!options || !options.message) {
            return;
        }

        if (options.key || options.name) {
            this.remove(options);
        }

        this._push(options);
    },

    clear: function() {
        this.messages = new DefineList();
    },

    remove: function(options) {
        if (!options || (!options.key && !options.name && !options.type)) {
            return;
        }

        this.messages = this.messages.filter(function(item) {
            var keep = true;

            if (options.key) {
                keep = item.key !== options.key;
            } else {
                if (options.name) {
                    keep = item.name !== options.name;
                } else {
                    if (options.type) {
                        keep = (item.type || 'info') !== options.type;
                    }
                }
            }

            return keep;
        });
    },

    _push: function(options, mode) {
        var key = this._key + 1;
        var self = this;
        var expiryDate = new Date();

        if (!options || !options.message) {
            return;
        }

        var type = options.type || 'info';

        expiryDate.setSeconds(expiryDate.getSeconds() + 10);

        const message = {
            message: stache.safeString(options.message),
            type: type,
            mode: mode,
            key: key,
            name: options.name,
            expiryDate: expiryDate,
            destroy: function() {
                self.remove({ key: key });
            }
        };

        this.messages.push(message);

        this._key = key;
    },

    _removeExpiredAlerts: function() {
        var self = this;
        var date = new Date();

        each(this.messages, function(item) {
            if (item.expiryDate && item.expiryDate < date) {
                item.destroy();
            }
        });

        setTimeout(() => {
            self._removeExpiredAlerts();
        }, 500);
    }
});

var alerts = new Alerts();

alerts._removeExpiredAlerts();

export default alerts;