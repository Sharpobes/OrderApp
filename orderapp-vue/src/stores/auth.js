import { defineStore } from 'pinia'
import api from '@/api/axios'

export const useAuthStore = defineStore('auth', {
    state: () => ({
        token: localStorage.getItem('token') || null,
        user: null,
        role: null,
        customerId: null
    }),

    getters: {
        isAuthenticated: (state) => !!state.token,
        isManager: (state) => state.role === 'Manager',
        isCustomer: (state) => state.role === 'Customer'
    },

    actions: {
        async login(login, password) {
            const response = await api.post('/auth/login', { login, password })
            const token = response.data.token
            this.token = token
            localStorage.setItem('token', token)
            this._parseToken(token)
        },

        async register(login, password) {
            await api.post('/auth/register', { login, password })
        },

        logout() {
            this.token = null
            this.role = null
            this.user = null
            this.customerId = null
            localStorage.removeItem('token')
        },

        initFromToken() {
            if (this.token) {
                this._parseToken(this.token)
            }
        },

        _parseToken(token) {
            try {
                const payload = JSON.parse(atob(token.split('.')[1]))
                console.log('Token payload:', payload)
                const roleClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
                this.role = Array.isArray(roleClaim) ? roleClaim[0] : roleClaim
                this.user = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']
                this.customerId = payload['CustomerId'] || null
                console.log('Role:', this.role)
                console.log('CustomerId:', this.customerId)
            } catch (e) {
                console.error('Ошибка парсинга токена:', e)
            }
        }
    }
})