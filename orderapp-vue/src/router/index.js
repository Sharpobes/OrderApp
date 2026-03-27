import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: '/',
            redirect: '/login'
        },
        {
            path: '/login',
            component: () => import('@/views/LoginView.vue'),
            meta: { guest: true } // только для неавторизованных
        },
        {
            path: '/register',
            component: () => import('@/views/RegisterView.vue'),
            meta: { guest: true }
        },
        {
            path: '/catalog',
            component: () => import('@/views/CatalogView.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/orders',
            component: () => import('@/views/OrdersView.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/manager',
            component: () => import('@/views/ManagerView.vue'),
            meta: { requiresAuth: true, role: 'Manager' }
        }
    ]
})

// Гард — защита роутов
router.beforeEach((to, from, next) => {
    const auth = useAuthStore()

    if (to.meta.requiresAuth && !auth.isAuthenticated) {
        next('/login')
    } else if (to.meta.guest && auth.isAuthenticated) {
        next('/catalog')
    } else if (to.meta.role && auth.role !== to.meta.role) {
        next('/catalog')
    } else {
        next()
    }
})

export default router