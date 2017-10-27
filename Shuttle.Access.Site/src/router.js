import DefineMap from 'can-define/map/';
import resources from '~/resources';
import alerts from '~/alerts';
import localisation from '~/localisation';
import security from '~/security';
import stache from 'can-stache';
import state from '~/state';

var Data = DefineMap.extend({
    resource: 'string',
    action: 'string',
    id: 'string',
    full: {
        get: function() {
            return this.resource + (!!this.id ? `/${this.id}` : '') + (!!this.action ? `/${this.action}` : '');
        }
    }
});

var Router = DefineMap.extend({
    data: { Value: Data },
    previousHash: 'string',

    init: function () {
        const self = this;

        this.data.on('full', function () {
            self.process.call(self);
        });
    },

    process: function () {
        var resource;
        var resourceName = this.data.resource;
        var actionName = this.data.action;
        var isActionRoute = !!actionName;
        var previousHash = this.previousHash;

        if ($('#application-content').length === 0) {
            return;
        }

        if (!resourceName) {
            return;
        }

        if (isActionRoute) {
            if (!actionName) {
                return;
            }

            resource = resources.find(resourceName, { action: actionName });
        } else {
            resource = resources.find(resourceName);
        }

        if (previousHash && previousHash === window.location.hash) {
            return;
        }

        this.previousHash = window.location.hash;

        if (!resource) {
            alerts.show({ message: localisation.value('exceptions.resource-not-found', { hash: window.location.hash, interpolation: { escape: false } }), type: 'warning', name: 'route-error' });

            return;
        }

        if (resource.permission && !security.hasPermission(resource.permission)) {
            alerts.show({ message: localisation.value('security.access-denied', { name: resource.name || window.location.hash, permission: resource.permission, interpolation: { escape: false } }), type: 'danger', name: 'route-error' });
	    
            return;
        }

        alerts.clear();
        state.controls.splice(0, state.controls.length);
        state.title = '';

        var componentName = resource.componentName || 'sentinel-' + resource.name + (isActionRoute ? `-${actionName}` : '');

        $('#application-content').html(stache('<' + componentName + '></' + componentName + '>')());
    },

    goto: function(href) {
        window.location.hash = (href.indexOf('#!') === -1 ? '#!' : '') + href;
    }
});

export default new Router();