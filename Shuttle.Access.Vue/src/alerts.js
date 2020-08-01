export default class Alerts {
    constructor() {
        this._key = 1;
        this.messages = [];

        this._removeExpiredAlerts();
    }

    add(alert) {
        if (!alert || !alert.message) {
            return;
        }
        if (alert.key || alert.name) {
            this.remove(alert);
        }
        this._push(alert);
    }

    clear() {
        this.messages = [];
    }

    remove(alert) {
        if (!alert || (!alert.key && !alert.name && !alert.type)) {
            return;
        }
        this.messages = this.messages.filter(function (item) {
            var keep = true;
            if (alert.key) {
                keep = item.key !== alert.key;
            }
            else {
                if (alert.name) {
                    keep = item.name !== alert.name;
                }
                else {
                    if (alert.type) {
                        keep = (item.type || 'info') !== alert.type;
                    }
                }
            }
            return keep;
        });
    }

    _push(alert, mode) {
        var key = this._key + 1;
        var expiryDate = new Date();
        if (!alert || !alert.message) {
            return;
        }
        var type = alert.type || 'info';
        expiryDate.setSeconds(expiryDate.getSeconds() + 10);
        const message = {
            message: alert.message,
            type: type,
            mode: mode,
            key: key,
            name: alert.name,
            expiryDate: expiryDate
        };
        this.messages.push(message);
        this._key = key;
    }

    _removeExpiredAlerts() {
        var self = this;
        var date = new Date();

        if (!this.messages) {
            return;
        }

        this.messages = this.messages.filter(function (message) {
            return (message.expiryDate &&
                message.expiryDate < date) ? undefined : message;
        });

        setTimeout(function() {
            self._removeExpiredAlerts();
        }, 500);
    }
}

