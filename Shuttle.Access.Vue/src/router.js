import Vue from 'vue'
import Router from 'vue-router'
import store from './store';

Vue.use(Router)

const router = new Router({
    mode: 'history',
    base: process.env.BASE_URL,
    routes: [
        {
            path: '/login',
            name: 'login',
            component: () => import(/* webpackChunkName: "login" */ './views/Login.vue')
        },
        {
            path: '*',
            redirect: '/login'
        }
    ]
})

const openRoutes = [
    '/login',
    '/register'
];

router.beforeEach((to, from, next) => {
    store.dispatch('fetchAccessToken');
    if (!openRoutes.includes(to.fullPath) && !store.state.accessToken) {
        next('/login');
    } else if (to.fullPath === '/login' && store.state.accessToken) {
        next('/search');
    }
    else {
        next();
    }
});

export default router;