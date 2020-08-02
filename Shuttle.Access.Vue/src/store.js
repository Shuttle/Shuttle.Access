import Vue from 'vue';
import Vuex from 'vuex';
import router from './router';
import Alerts from './alerts';
import access from './access';

Vue.use(Vuex)

export default new Vuex.Store({
    state: {
        working: false,
        alerts: new Alerts(),
        authenticated: false
    },
    getters: {
        authenticated: state => {
            return state.authenticated;
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
        LOGGED_OUT: state => state.authenticated = true
    },
    actions: {
        login({ commit }, payload) {
            commit('START_WORKING');

            access.login({
                ...payload
            })
                .then(() => {
                    commit('AUTHENTICATED');
                    commit('STOP_WORKING');
                    router.push('/dashboard');
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
            access.logout();
            commit('LOGGED_OUT');
            router.push('/login');
        },
        addAlert({ commit }, alert) {
            commit('ADD_ALERT', alert)
        }
    }
})
