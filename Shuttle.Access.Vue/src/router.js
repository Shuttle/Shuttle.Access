import Vue from 'vue'
import Router from 'vue-router'
import access from './access';

Vue.use(Router)

const router = new Router({
    mode: 'history',
    base: process.env.BASE_URL,
    routes: [
        {
            path: '/dashboard',
            name: 'dashboard',
            component: () => import(/* webpackChunkName: "dashboard" */ './views/Dashboard.vue')
        },
        {
            path: '/login',
            name: 'login',
            component: () => import(/* webpackChunkName: "login" */ './views/Login.vue')
        },
        {
            path: '/register',
            name: 'register',
            component: () => import(/* webpackChunkName: "register" */ './views/Register.vue')
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
    if (!openRoutes.includes(to.fullPath) && access.loginStatus !== 'logged-in') {
        next('/login');
    } else if (to.fullPath === '/login' && access.loginStatus === 'logged-in') {
        next('/home');
    }
    else {
        next();
    }
});

export default router;