import $ from 'jquery';
import 'popper.js';
import 'bootstrap';
import 'moment';
import 'tempusdominus';
import '@fortawesome/fontawesome-svg-core';

import 'bootstrap/dist/css/bootstrap.css';
import 'tempusdominus-bootstrap-4/build/css/tempusdominus-bootstrap-4.css';
import './styles.css!';

import {config as faConfiguration, library, dom} from '@fortawesome/fontawesome-svg-core'
import {fas} from '@fortawesome/free-solid-svg-icons'
import {far} from '@fortawesome/free-regular-svg-icons'
import {options as apiOptions} from 'shuttle-can-api';
import loader from '@loader';

import stache from '~/main.stache!';
import localisation from '~/localisation';
import state from '~/state';
import router from '~/router';

import canstrap from 'shuttle-canstrap';
import access from 'shuttle-access';

import '~/navigation/';
import '~/dashboard/';
import '~/role/';
import '~/user/';

apiOptions.url = loader.serviceBaseURL;
access.url = loader.serviceBaseURL;
faConfiguration.autoReplaceSvg = 'nest';

library.add(fas, far)

dom.watch();

$.ajaxPrefilter(function (options, originalOptions) {
    options.error = function (xhr) {
        if (xhr.status != 200) {
            if (xhr.responseJSON) {
                state.alerts.add({message: xhr.responseJSON.message, type: 'danger', name: 'ajax-prefilter-error'});
            } else {
                state.alerts.add({
                    message: xhr.status + ' / ' + xhr.statusText,
                    type: 'danger',
                    name: 'ajax-prefilter-error'
                });
            }
        }

        if (originalOptions.error) {
            originalOptions.error(xhr);
        }
    };
});

canstrap.button.remove.confirmation = function (options) {
    state.modal.confirmation.show(options);
}

localisation.start(function (error) {
    if (error) {
        throw new Error(error);
    }

    access.start()
        .then(function () {
            router.start();

            $('#application-container').html(stache(state));

            if (window.location.hash === '#!' || !window.location.hash) {
                if (access.isUserRequired) {
                    router.goto({resource: 'user', action: 'register'});
                }
                else {
                    router.goto({resource: 'dashboard'});
                }
            }

            router.process();
        });
});
