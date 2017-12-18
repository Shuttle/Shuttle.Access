import DefineMap from 'can-define/map/';
import resources from '~/resources';
import localisation from '~/localisation';
import security from '~/security';
import stache from 'can-stache';
import route from 'can-route';
import state from '~/state';
import guard from 'shuttle-guard';

var RouteData = DefineMap.extend({
    resource: {
        type:'string',
        value: ''
    },
    action: {
        type:'string',
        value: ''
    },
    id: {
        type:'string',
        value: ''
    },
    full: {
        get: function () {
            return this.resource + (!!this.id ? `/${this.id}` : '') + (!!this.action ? `/${this.action}` : '');
        }
    }
});

var routeData = new RouteData();

routeData.on('full', function(ev, newVal, oldVal){
    if (!security.isUserRequired || (this.resource === 'user' && this.action === 'register')) {
        return;
    }

    this.update({resource: 'user', action: 'register'}, true);
});

var Router = DefineMap.extend({
    data: {
        Type: RouteData,
        value: routeData
    },
    previousHash: 'string',

    init: function () {
        const self = this;

        this.data.on('full', function () {
            self.process.call(self);
        });
    },

    start: function () {
        route('{resource}');
        route('{resource}/{action}');
        route('{resource}/{id}/{action}');

        route.data = this.data;

        route.start();
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

            resource = resources.find(resourceName, {action: actionName});
        } else {
            resource = resources.find(resourceName);
        }

        if (previousHash && previousHash === window.location.hash) {
            return;
        }

        this.previousHash = window.location.hash;

        if (!resource) {
            state.alerts.show({
                message: localisation.value('exceptions.resource-not-found', {
                    hash: window.location.hash,
                    interpolation: {escape: false}
                }), type: 'warning', name: 'route-error'
            });

            return;
        }

        if (resource.permission && !security.hasPermission(resource.permission)) {
            state.alerts.show({
                message: localisation.value('security.access-denied', {
                    name: resource.name || window.location.hash,
                    permission: resource.permission,
                    interpolation: {escape: false}
                }), type: 'danger', name: 'route-error'
            });

            return;
        }

        state.alerts.clear();
        state.navbarControls.splice(0, state.navbarControls.length);
        state.title = '';

        var componentName = resource.componentName || 'access-' + resource.name + (isActionRoute ? `-${actionName}` : '');

        $('#application-content').html(stache('<' + componentName + '></' + componentName + '>')());
    },

    goto: function (data) {
        guard.againstUndefined(data, 'data');

        if (typeof(data) !== 'object') {
            throw new Error('Call \'router.goto\' with route data: e.g. router.goto({resource: \'the-resource\', action: \'the-action\'});');
        }

        if (!data.resource) {
            throw new Error('The \'data\' argument does not contain a \'resource\' value.')
        }

        route.data.update(data, true);
    }
});

export default new Router();