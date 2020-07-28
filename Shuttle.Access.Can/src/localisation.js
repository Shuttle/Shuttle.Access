import stache from 'can-stache';
import i18next from 'i18next';
import backend from 'i18next-xhr-backend';
import loader from '@loader';
import i18n from 'shuttle-canstrap/infrastructure/i18n';

let localisation = {
    _initialised: false,
    _namespaces: ['access', 'navigation'],

    start: function(callback) {
        stache.addHelper('i18n', function (key) {
            return i18next.t(key);
        });

        i18next
            .use(backend)
            .init({
                backend: {
                    loadPath: `locales/{{lng}}/{{ns}}.json?_${loader.localeVersion}`
                },
                debug: loader.debug || false,
                lng: 'en',
                fallbackLng: 'en',
                ns: this._namespaces,
                defaultNS: 'access'
            }, (error) => {
                if (!error) {
                    this._initialised = true;
                }

                callback(error);
            });

        i18n.wire(this);
    },

    addNamespace: function(ns, callback) {
        if (this._initialised) {
            i18next.loadNamespaces(ns, callback);
        } else {
            this._namespaces.push(ns);
        }
    },

    value: function (key, options) {
        return i18next.t(key, options);
    }
};

export default localisation;