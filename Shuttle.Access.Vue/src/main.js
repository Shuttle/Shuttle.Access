import Vue from 'vue';
import store from './store'
import App from './App.vue';
import BootstrapVue from 'bootstrap-vue';
import Vuelidate from 'vuelidate';
import { library } from '@fortawesome/fontawesome-svg-core'
import { faCircleNotch, faExternalLinkAlt, faUser } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import router from './router'
import Title from './components/sa-title';
import { AgGridVue } from 'ag-grid-vue';
import access from './access';

library.add(faCircleNotch, faExternalLinkAlt, faUser);

Vue.component('font-awesome-icon', FontAwesomeIcon);
Vue.component('ag-grid-vue', AgGridVue);
Vue.component('sa-title', Title);

Vue.use(BootstrapVue);
Vue.use(Vuelidate);

Vue.config.productionTip = false;

new Vue({
  store,
  router,
  render: h => h(App),
}).$mount('#app');

access.initialize()
  .catch(function () {
    store.dispatch('addAlert', { 
      message: 'Could not initialize the `shuttle-access` module.  The back-end may not be available.',
      type: 'danger'
    })
  });
