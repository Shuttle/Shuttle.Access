import Vue from 'vue';
import store from './store'
import App from './App.vue';
import i18n from './i18n'
import BootstrapVue from 'bootstrap-vue';
import Vuelidate from 'vuelidate';
import { library } from '@fortawesome/fontawesome-svg-core'
import { faCircleNotch, faExternalLinkAlt, faUser } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import router from './router'
import Title from './components/sv-title';
import { AgGridVue } from 'ag-grid-vue';
import access from './access';
import configuration from './configuration';
import axios from 'axios';

library.add(faCircleNotch, faExternalLinkAlt, faUser);

Vue.component('font-awesome-icon', FontAwesomeIcon);
Vue.component('ag-grid-vue', AgGridVue);
Vue.component('sv-title', Title);

Vue.use(BootstrapVue);
Vue.use(Vuelidate);

Vue.config.productionTip = false;

Vue.prototype.$axios = axios.create({ baseURL: configuration.url });

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
  })
  .catch(function () {
    store.dispatch('addAlert', {
      message: this.$t('exception.access-failure'),
      type: 'danger'
    });
  });
