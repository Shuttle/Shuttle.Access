import Vue from 'vue';
import store from './store'
import App from './App.vue';
import i18n from './i18n'
import BootstrapVue from 'bootstrap-vue';
import ShuttleVue from 'shuttle-vue';
import Vuelidate from 'vuelidate';
import { library } from '@fortawesome/fontawesome-svg-core'
import { faCircleNotch, faSignOutAlt, faUser, faPlusSquare, faSyncAlt } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import router from './router'
import { AgGridVue } from 'ag-grid-vue';
import access from './access';
import configuration from './configuration';
import axios from 'axios';

library.add(faCircleNotch, faSignOutAlt, faUser, faPlusSquare, faSyncAlt);

Vue.component('font-awesome-icon', FontAwesomeIcon);
Vue.component('ag-grid-vue', AgGridVue);

Vue.use(BootstrapVue);
Vue.use(ShuttleVue);
Vue.use(Vuelidate);

Vue.config.productionTip = false;

Vue.prototype.$api = axios.create({ baseURL: configuration.url });

new Vue({
  store,
  router,
  i18n,
  render: h => h(App),
}).$mount('#app');

access.initialize()
  .then(function () {
    if (access.isUserRequired) {
      router.push('register');
    }
    else {
      if (access.loginStatus == 'logged-in') {
        store.commit('AUTHENTICATED');
      }else{
        router.push('login');
      }
    }
  })
  .catch(function () {
    store.dispatch('addAlert', {
      message: i18n.t('exceptions.access-failure'),
      type: 'danger'
    });
  });
