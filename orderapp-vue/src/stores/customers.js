import { defineStore } from 'pinia'
import api from '@/api/axios'

export const useCustomersStore = defineStore('customers', {
    state: () => ({
        customers: [],
        loading: false
    }),

    actions: {
        async fetchAll() {
            this.loading = true
            try {
                const res = await api.get('/customers')
                this.customers = res.data
            } finally {
                this.loading = false
            }
        },

        async create(customer) {
            const res = await api.post('/customers', customer)
            this.customers.push(res.data)
        },

        async update(id, customer) {
            await api.put(`/customers/${id}`, customer)
            const index = this.customers.findIndex(c => c.id === id)
            if (index !== -1) this.customers[index] = { ...this.customers[index], ...customer }
        },

        async remove(id) {
            await api.delete(`/customers/${id}`)
            this.customers = this.customers.filter(c => c.id !== id)
        }
    }
})