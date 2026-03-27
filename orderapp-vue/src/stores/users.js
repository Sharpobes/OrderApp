import { defineStore } from 'pinia'
import api from '@/api/axios'

export const useUsersStore = defineStore('users', {
    state: () => ({
        users: [],
        loading: false
    }),

    actions: {
        async fetchAll() {
            this.loading = true
            try {
                const res = await api.get('/users')
                this.users = res.data
            } finally {
                this.loading = false
            }
        },

        async setRole(id, role) {
            await api.post(`/users/${id}/roles`, { role })
            const user = this.users.find(u => u.id === id)
            if (user) user.roles = [role]
        },

        async setCustomer(id, customerId) {
            await api.post(`/users/${id}/customer`, { customerId })
        },

        async remove(id) {
            await api.delete(`/users/${id}`)
            this.users = this.users.filter(u => u.id !== id)
        }
    }
})