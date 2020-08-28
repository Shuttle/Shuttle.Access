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
            path: '/roles',
            name: 'roles',
            component: () => import(/* webpackChunkName: "roles" */ './views/Roles.vue')
        },
        {
            path: '/users',
            name: 'users',
            component: () => import(/* webpackChunkName: "users" */ './views/Users.vue')
        },
        {
            path: '/profile',
            name: 'profile',
            component: () => import(/* webpackChunkName: "profile" */ './views/Profile.vue')
        },
        {
            path: '/register',
            name: 'register',
            component: () => import(/* webpackChunkName: "register" */ './views/Register.vue')
        },
        {
            path: '*',
            redirect: '/users'
        }
    ]
})

const openRoutes = [
    '/login',
    '/register'
];

router.beforeEach((to, from, next) => {
    if (!access.initialized) {
        return;
    }

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