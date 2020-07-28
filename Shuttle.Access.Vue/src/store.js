import Vue from 'vue';
import Vuex from 'vuex';
import axios from 'axios';
import configuration from "@/configuration.js";
import router from './router';
import Alerts from './alerts';

Vue.use(Vuex)

export default new Vuex.Store({
    state: {
        accessToken: null,
        working: false,
        alerts: new Alerts()
    },
    getters: {
        authenticated: state => {
            return state.accessToken !== null;
        }
    },
    mutations: {
        loginStart: state => state.working = true,
        loginStop: (state, message) => {
            state.working = false;
            state.alerts.add({
                message: message,
                name: "login-error",
                type: "danger"
            });
        },
        updateAccessToken: (state, accessToken) => {
            state.accessToken = accessToken;
        },
        logout: (state) => {
            state.accessToken = null;
        },
        removeAlert: (state, alert) => {
            state.alerts.remove(alert);
        }
    },
    actions: {
        login({ commit }, payload) {
            commit('loginStart');

            axios.post(configuration.url + '/token', {
                ...payload
            })
                .then(response => {
                    localStorage.setItem('accessToken', response.data);
                    commit('loginStop', null);
                    commit('updateAccessToken', response.data);
                    router.push('/search');
                })
                .catch(error => {
                    commit('loginStop', error.response.data);
                    commit('updateAccessToken', null);
                })
        },
        fetchAccessToken({ commit }) {
            commit('updateAccessToken', localStorage.getItem('accessToken'));
        },
        logout({ commit }) {
            localStorage.removeItem('accessToken');
            commit('logout');
            router.push('/login');
        }
    }
})
