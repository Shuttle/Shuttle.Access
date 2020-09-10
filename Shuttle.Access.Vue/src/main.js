import Vue from 'vue';
import store from './store'
import App from './App.vue';
import i18n from './i18n'
import BootstrapVue from 'bootstrap-vue';
import ShuttleVue from 'shuttle-vue';
import Vuelidate from 'vuelidate';
import { library } from '@fortawesome/fontawesome-svg-core'
import { faCircleNotch, faEye, faEyeSlash, faKey, faHourglass, faSignOutAlt, faUser, faPlusSquare, faShieldAlt, faSyncAlt, faTrashAlt, faUserCircle } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import router from './router'
import access from './access';
import configuration from './configuration';
import axios from 'axios';

library.add(faCircleNotch, faEye, faEyeSlash, faKey, faHourglass, faSignOutAlt, faUser, faPlusSquare, faShieldAlt, faSyncAlt, faTrashAlt, faUserCircle);

Vue.component('font-awesome-icon', FontAwesomeIcon);

Vue.use(BootstrapVue);
Vue.use(ShuttleVue);
Vue.use(Vuelidate);

Vue.config.productionTip = false;

Vue.prototype.$api = axios.create({ baseURL: configuration.url });
Vue.prototype.$api.interceptors.request.use(function (config) {
  config.headers['access-sessiontoken'] = access.token;

  return config;
});
Vue.prototype.$api.interceptors.response.use((response) => response, (error) => {
  store.dispatch('addAlert', {
    message: error.response.data,
    type: 'danger'
  });
});

Vue.prototype.$access = access;

const vm = new Vue({
  store,
  router,
  i18n,
  render: h => h(App),
}).$mount('#app');

var redirect = true;

access.initialize()
  .then(function () {
    if (access.isUserRequired) {
      redirect = false;
      router.push('register');
    }
    else {
      if (access.loginStatus == 'logged-in') {
        store.commit('AUTHENTICATED');
      } else {
        redirect = false;
        router.push('login');
      }
    }
  })
  .catch(function () {
    store.dispatch('addAlert', {
      message: i18n.t('exceptions.access-failure'),
      type: 'danger'
    });
  })
  .finally(function () {
    if (redirect) {
      router.push({ path: window.location.pathname });
    }
  });
