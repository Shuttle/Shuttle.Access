import Vue from 'vue';
import Vuex from 'vuex';
import Alerts from './alerts';
import access from './access';
import api from './api';

Vue.use(Vuex)

export default new Vuex.Store({
    state: {
        working: false,
        alerts: new Alerts(),
        authenticated: false,
        secondaryNavbarItems: []
    },
    getters: {
        authenticated: state => {
            return state.authenticated;
        },
        secondaryNavbarItems: state => {
            return state.secondaryNavbarItems;
        }
    },
    mutations: {
        START_WORKING: state => state.working = true,
        STOP_WORKING: state => state.working = false,
        ADD_ALERT: (state, alert) => {
            state.alerts.add(alert);
        },
        REMOVE_ALERT: (state, alert) => {
            state.alerts.remove(alert);
        },
        AUTHENTICATED: state => state.authenticated = true,
        LOGGED_OUT: state => state.authenticated = false,
        ADD_SECONDARY_NAVBAR_ITEM: (state, item) => {
            state.secondaryNavbarItems.push(item);
        },
        CLEAR_SECONDARY_NAVBAR_ITEMS: (state) => {
            state.secondaryNavbarItems = [];
        }
    },
    actions: {
        login({ commit }, payload) {
            commit('START_WORKING');

            return access.login({
                ...payload
            })
                .then(() => {
                    commit('AUTHENTICATED');
                    commit('STOP_WORKING');
                })
                .catch(error => {
                    commit('ADD_ALERT', {
                        message: error,
                        type: 'danger'
                    });

                    commit('STOP_WORKING');
                })
        },
        logout({ commit }) {
            return api.delete('sessions').then(
                function () {
                    access.logout();
                    commit('LOGGED_OUT');
                });
        },
        addAlert({ commit }, alert) {
            commit('ADD_ALERT', alert);
        },
        addSecondaryNavbarItem({ commit }, item) {
            if (!item.click || typeof item.click !== "function") {
                throw new Error("Secondary navbar item does not have a 'click' method defined.")
            }

            commit('ADD_SECONDARY_NAVBAR_ITEM', item);
        },
        clearSecondaryNavbarItems({ commit }) {
            commit('CLEAR_SECONDARY_NAVBAR_ITEMS');
        }
    }
})
