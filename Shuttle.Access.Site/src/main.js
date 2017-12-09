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
import route from 'can-route';

import 'shuttle-canstrap';

import '~/navigation/';
import '~/dashboard/';
import '~/role/';
import '~/user/';

localisation.start(function(error) {
    if (error) {
        throw new Error(error);
    }

    security.start()
        .then(function() {
            route('{resource}');
            route('{resource}/{action}');
            route('{resource}/{id}/{action}');

            route.data = router.data;

            route.ready();
        })
        .then(function() {
            $('#application-container').html(stache(state));

            if (window.location.hash === '#!' || !window.location.hash) {
                window.location.hash = security.isUserRequired
                                           ? '#!user/register'
                                           : '#!dashboard';
            }

            router.process();
        });
});
