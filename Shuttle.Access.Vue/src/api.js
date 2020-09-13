import axios from 'axios';
import store from './store';
import configuration from './configuration';
import access from './access';
import router from './router';

var api = axios.create({ baseURL: configuration.url });

api.interceptors.request.use(function (config) {
    config.headers['access-sessiontoken'] = access.token;

    return config;
});

api.interceptors.response.use((response) => response, (error) => {
    if (error.response.status == 401) {
        router.push("login");

        return error;
    }

    store.dispatch('addAlert', {
        message: error.response.data,
        type: 'danger'
    });

    return error;
});

export default api;