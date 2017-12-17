import $ from 'jquery';
import 'popper.js';
import 'bootstrap';
import 'can-stache/helpers/route';

import 'bootstrap/dist/css/bootstrap.css!';
import 'font-awesome/css/font-awesome.css';
import './styles.css!';

import stache from '~/main.stache!';
import localisation from '~/localisation';
import security from '~/security';
import state from '~/state';
import router from '~/router';

import 'shuttle-canstrap';

import '~/navigation/';
import '~/dashboard/';
import '~/role/';
import '~/user/';

$.ajaxPrefilter(function (options, originalOptions) {
    options.error = function (xhr) {
        if (xhr.responseJSON) {
            state.alerts.show({message: xhr.responseJSON.message, type: 'danger', name: 'ajax-prefilter-error'});
        } else {
            state.alerts.show({
                message: xhr.status + ' / ' + xhr.statusText,
                type: 'danger',
                name: 'ajax-prefilter-error'
            });
        }

        if (originalOptions.error) {
            originalOptions.error(xhr);
        }
    };
});

localisation.start(function (error) {
    if (error) {
        throw new Error(error);
    }

    security.start()
        .then(function () {
            router.start();

            $('#application-container').html(stache(state));

            if (window.location.hash === '#!' || !window.location.hash) {
                if (security.isUserRequired) {
                    router.goto({resource: 'user', action: 'register'});
                }
                else {
                    router.goto({resource: 'dashboard'});
                }
            }

            router.process();
        });
});
