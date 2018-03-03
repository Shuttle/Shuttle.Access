import DefineMap from 'can-define/map/';
import DefineList from 'can-define/list/';
import Component from 'can-component';
import view from './navigation.stache!';
import each from 'can-util/js/each/';
import {DropdownMap, DropdownList} from 'shuttle-canstrap/nav-dropdown/';
import map from './navigation-map';
import security from '~/security';
import state from '~/state';
import router from '~/router';

var ViewModel = DefineMap.extend({
    hasSecondary(){
        return !!this.title || this.navbar.controls.length > 0;
    },
    security: {
        value: security
    },
    title: {
        get() {
            return state.title;
        }
    },
    navbar: {
        value: state.navbar
    },
    resources: {
        get: function (value) {
            var result = new DefineList();

            each(map, function (item) {
                var add = false;
                var list = new DropdownList();

                if (!item.permission || security.hasPermission(item.permission)) {
                    if (item.items !== undefined) {
                        each(item.items, function (subitem) {
                            if (!subitem.permission || security.hasPermission(subitem.permission)) {
                                add = true;

                                list.push(new DropdownMap({
                                    href: subitem.href,
                                    text: subitem.text
                                }));
                            }
                        });
                    } else {
                        add = true;
                    }

                    if (add) {
                        result.push({
                            text: item.text,
                            href: item.href,
                            list: list
                        });
                    }
                }
            });

            return result;
        }
    },
    logout() {
        this.security.logout();
        router.goto({resource: 'dashboard'});
    }
});

export default Component.extend({
    tag: 'access-navigation',
    view,
    ViewModel
});