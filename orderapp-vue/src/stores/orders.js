import { defineStore } from 'pinia'
import api from '@/api/axios'

export const useOrdersStore = defineStore('orders', {
    state: () => ({
        orders: [],
        loading: false,
        error: null
    }),

    actions: {
        async fetchAll() {
            this.loading = true
            try {
                const res = await api.get('/orders')
                this.orders = res.data
            } catch (e) {
                this.error = 'Ошибка загрузки заказов'
            } finally {
                this.loading = false
            }
        },

        async fetchMy() {
            this.loading = true
            try {
                const res = await api.get('/orders/my')
                this.orders = res.data
            } catch (e) {
                this.error = 'Ошибка загрузки заказов'
            } finally {
                this.loading = false
            }
        },

        async create(items) {
            const res = await api.post('/orders', { items })
            this.orders.push(res.data)
        },

        async confirm(id, shipmentDate) {
            await api.put(`/orders/${id}/confirm`, { shipmentDate })
            const order = this.orders.find(o => o.id === id)
            if (order) {
                order.status = 'Выполняется'
                order.shipmentDate = shipmentDate
            }
        },

        async complete(id) {
            await api.put(`/orders/${id}/complete`)
            const order = this.orders.find(o => o.id === id)
            if (order) order.status = 'Выполнен'
        },

        async remove(id) {
            await api.delete(`/orders/${id}`)
            this.orders = this.orders.filter(o => o.id !== id)
        }
    }
})